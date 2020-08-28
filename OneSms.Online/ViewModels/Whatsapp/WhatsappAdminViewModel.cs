using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using OneSms.Online.Data;
using OneSms.Online.Hubs;
using OneSms.Online.Services;
using OneSms.Web.Shared.Constants;
using OneSms.Web.Shared.Dtos;
using OneSms.Web.Shared.Enumerations;
using OneSms.Web.Shared.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace OneSms.Online.ViewModels.Whatsapp
{
    public class WhatsappAdminViewModel: ReactiveObject
    {
        private OneSmsDbContext _oneSmsDbContext;
        private ServerConnectionService _serverConnectionService;
        private IHubContext<OneSmsHub> _oneSmsHubContext;
        private HubEventService _hubEventService;

        public WhatsappAdminViewModel(OneSmsDbContext oneSmsDbContext, ServerConnectionService serverConnectionService, IHubContext<OneSmsHub> oneSmsHubContext, HubEventService hubEventService)
        {
            _oneSmsDbContext = oneSmsDbContext;
            _serverConnectionService = serverConnectionService;
            _oneSmsHubContext = oneSmsHubContext;
            _hubEventService = hubEventService;
            Transactions = new ObservableCollection<WhatsappTransaction>();
            MobileServers = new ObservableCollection<ServerMobile>();

            LoadMobileServers = ReactiveCommand.CreateFromTask(() => _oneSmsDbContext.MobileServers.Include(x=> x.Sims).ThenInclude(x => x.Apps).ToListAsync());
            LoadMobileServers.Do(servers => MobileServers = new ObservableCollection<ServerMobile>(servers)).Subscribe();
            LoadMobileServers.ThrownExceptions.Select(x => x.Message).ToPropertyEx(this, x => x.Errors);

            AddTransaction = ReactiveCommand.CreateFromTask<WhatsappTransaction, WhatsappTransaction>(async entity =>
            {
                entity.CreatedOn = DateTime.UtcNow;
                entity.CompletedTime = DateTime.UtcNow;
                entity.StartTime = DateTime.UtcNow;
                entity.OneSmsAppId = MobileServer.Sims.FirstOrDefault().Apps.FirstOrDefault().AppId;
               
                Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<WhatsappTransaction> created = _oneSmsDbContext.WhatsappTransactions.Add(entity);
                await _oneSmsDbContext.SaveChangesAsync();
                entity.Id = created.Entity.Id;
                var mobileServer = await _oneSmsDbContext.MobileServers.FirstAsync(x => x.Id == entity.MobileServerId);
                entity.MobileServer = mobileServer;
                return entity;
            });
            SendTransactionToMobileServer = ReactiveCommand.CreateFromTask<WhatsappTransaction, Unit>(async transaction =>
            {
                var serverKey = transaction.MobileServer.Key.ToString();
                LatestTransaction.WhatsappId = transaction.Id;
                LatestTransaction.TransactionState = transaction.TransactionState;
                LatestTransaction.TimeStamp = DateTime.UtcNow;
                LatestTransaction.Message = transaction.Body;
                LatestTransaction.ImageLinks = new List<string> { transaction.ImageLinkOne, transaction.ImageLinkTwo, transaction.ImageLinkThree };
                LatestTransaction.MobileServerId = transaction.MobileServerId;
                LatestTransaction.ReceiverNumber = transaction.RecieverNumber;
                LatestTransaction.MessageTransactionProcessor = MessageTransactionProcessor.Whatsapp;
                LatestTransaction.TransactionId = transaction.TransactionId;
                var serverConnectionId = string.Empty;
                if (_serverConnectionService.ConnectedServers.TryGetValue(serverKey, out serverConnectionId))
                    await _oneSmsHubContext.Clients.Client(serverConnectionId).SendAsync(SignalRKeys.SendWhatsapp, LatestTransaction);
                return Unit.Default;

            });
            AddTransaction.InvokeCommand(SendTransactionToMobileServer);
            AddTransaction.Do(transaction => Transactions.Add(transaction)).Subscribe();

            SendTransactionToMobileServer.ThrownExceptions.Select(x => x.Message).ToPropertyEx(this, x => x.Errors);
            AddTransaction.ThrownExceptions.Select(x => x.Message).ToPropertyEx(this, x => x.Errors);

            _hubEventService.OnMessageStateChanged.ObserveOn(RxApp.MainThreadScheduler).Subscribe(message =>
            {
                var item = Transactions.FirstOrDefault(x => x.Id == message.WhatsappId);
                if (item != null)
                {
                    item.CompletedTime = message.TimeStamp;
                    item.TransactionState = message.TransactionState;
                    Transactions.Remove(item);
                    Transactions.Add(item);
                    Transactions = new ObservableCollection<WhatsappTransaction>(Transactions.OrderByDescending(x => x.CompletedTime));
                }
            });
        }

        [Reactive]
        public ObservableCollection<ServerMobile> MobileServers { get; set; }

        [Reactive]
        public ObservableCollection<WhatsappTransaction> Transactions { get; set; }

        public string Errors { [ObservableAsProperty]get; }

        public ServerMobile MobileServer { get; set; }

        public MessageTransactionProcessDto LatestTransaction { get; set; } = new MessageTransactionProcessDto();

        public ReactiveCommand<Unit, List<ServerMobile>> LoadMobileServers { get; }

        public ReactiveCommand<WhatsappTransaction, WhatsappTransaction> AddTransaction { get; }

        public ReactiveCommand<WhatsappTransaction, Unit> SendTransactionToMobileServer { get; }
    }
}
