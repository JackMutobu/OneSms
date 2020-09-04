using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using OneSms.Contracts.V1;
using OneSms.Contracts.V1.MobileServerRequest;
using OneSms.Data;
using OneSms.Domain;
using OneSms.Hubs;
using OneSms.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace OneSms.ViewModels.Whatsapp
{
    public class WhatsappAdminViewModel: ReactiveObject
    {
        private DataContext _dataContext;
        private IServerConnectionService _serverConnectionService;
        private IHubContext<OneSmsHub> _hubContext;
        private HubEventService _hubEventService;

        public WhatsappAdminViewModel(DataContext dataContext, IServerConnectionService serverConnectionService, IHubContext<OneSmsHub> hubContext, HubEventService hubEventService)
        {
            _dataContext = dataContext;
            _serverConnectionService = serverConnectionService;
            _hubContext = hubContext;
            _hubEventService = hubEventService;
            Transactions = new ObservableCollection<WhatsappMessage>();
            MobileServers = new ObservableCollection<MobileServer>();

            LoadMobileServers = ReactiveCommand.CreateFromTask(() => _dataContext.MobileServers.Include(x=> x.Sims).ThenInclude(x => x.Apps).ToListAsync());
            LoadMobileServers.Do(servers => MobileServers = new ObservableCollection<MobileServer>(servers)).Subscribe();
            LoadMobileServers.Do(servers => MobileServer = servers.FirstOrDefault()).Subscribe();
            LoadMobileServers.ThrownExceptions.Select(x => x.Message).ToPropertyEx(this, x => x.Errors);

            AddTransaction = ReactiveCommand.CreateFromTask<WhatsappMessage, WhatsappMessage>(async entity =>
            {
                entity.CompletedTime = DateTime.UtcNow;
                entity.StartTime = DateTime.UtcNow;
                entity.AppId = MobileServer.Sims.FirstOrDefault().Apps.FirstOrDefault().AppId;
               
                Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<WhatsappMessage> created = _dataContext.WhatsappMessages.Add(entity);
                await _dataContext.SaveChangesAsync();
                entity.Id = created.Entity.Id;
                var mobileServer = await _dataContext.MobileServers.FirstAsync(x => x.Id == entity.MobileServerId);
                entity.MobileServer = mobileServer;
                return entity;
            });
            SendTransactionToMobileServer = ReactiveCommand.CreateFromTask<WhatsappMessage, Unit>(async transaction =>
            {
                var serverKey = transaction.MobileServerId.ToString();
                LatestTransaction.WhatsappId = transaction.Id;
                LatestTransaction.MessageStatus = transaction.MessageStatus;
                LatestTransaction.Body = transaction.Body;
                LatestTransaction.ImageLinks = new List<string> { transaction.ImageLinkOne, transaction.ImageLinkTwo, transaction.ImageLinkThree };
                LatestTransaction.MobileServerId = transaction.MobileServerId;
                LatestTransaction.ReceiverNumber = transaction.RecieverNumber;
                LatestTransaction.TransactionId = transaction.TransactionId;
                LatestTransaction.AppId = transaction.AppId;
                LatestTransaction.TransactionId = transaction.TransactionId;
                var serverConnectionId = string.Empty;
                if (_serverConnectionService.ConnectedServers.TryGetValue(serverKey, out serverConnectionId))
                    await _hubContext.Clients.Client(serverConnectionId).SendAsync(SignalRKeys.SendWhatsapp, LatestTransaction);
                return Unit.Default;

            });
            AddTransaction.InvokeCommand(SendTransactionToMobileServer);
            AddTransaction.Do(transaction => Transactions.Add(transaction)).Subscribe();

            SendTransactionToMobileServer.ThrownExceptions.Select(x => x.Message).ToPropertyEx(this, x => x.Errors);
            AddTransaction.ThrownExceptions.Select(x => x.Message).ToPropertyEx(this, x => x.Errors);

            _hubEventService.OnWhatsappMessageStatusChanged.ObserveOn(RxApp.MainThreadScheduler).Subscribe(message =>
            {
                var item = Transactions.FirstOrDefault(x => x.Id == message.Id);
                if (item != null)
                {
                    item.CompletedTime = message.CompletedTime;
                    item.MessageStatus = message.MessageStatus;
                    Transactions.Remove(item);
                    Transactions.Add(item);
                    Transactions = new ObservableCollection<WhatsappMessage>(Transactions.OrderByDescending(x => x.CompletedTime));
                }
            });
        }

        [Reactive]
        public ObservableCollection<MobileServer> MobileServers { get; set; } = new ObservableCollection<MobileServer>();

        [Reactive]
        public ObservableCollection<WhatsappMessage> Transactions { get; set; } = new ObservableCollection<WhatsappMessage>();

        public string? Errors { [ObservableAsProperty]get; }

        public MobileServer MobileServer { get; set; } = null!;

        public WhatsappRequest LatestTransaction { get; set; } = new WhatsappRequest();

        public ReactiveCommand<Unit, List<MobileServer>> LoadMobileServers { get; }

        public ReactiveCommand<WhatsappMessage, WhatsappMessage> AddTransaction { get; }

        public ReactiveCommand<WhatsappMessage, Unit> SendTransactionToMobileServer { get; }
    }
}
