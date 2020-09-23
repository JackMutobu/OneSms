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

namespace OneSms.ViewModels
{
    public class SmsAdminViewModel:ReactiveObject
    {
        private DataContext _dataContext;
        private IServerConnectionService _serverConnectionService;
        private IHubContext<OneSmsHub> _hubContext;
        private HubEventService _hubEventService;

        public SmsAdminViewModel(DataContext dataContext,IServerConnectionService serverConnectionService,IHubContext<OneSmsHub> hubContext,HubEventService hubEventService)
        {
            _dataContext = dataContext;
            _serverConnectionService = serverConnectionService;
            _hubContext = hubContext;
            _hubEventService = hubEventService;
            SmsTransactions = new ObservableCollection<SmsMessage>();
            Sims = new ObservableCollection<SimCard>();
            MobileServers = new ObservableCollection<MobileServer>();
            SelectedSimCard = new SimCard();

            LoadMobileServers = ReactiveCommand.CreateFromTask(() => _dataContext.MobileServers.ToListAsync());

            LoadSimCards = ReactiveCommand.CreateFromTask(() => _dataContext.Sims.Include(x => x.MobileServer).Include(x => x.Apps).ToListAsync());
            LoadSimCards.Do(sims => Sims = new ObservableCollection<SimCard>(sims)).Subscribe();
            AddSmsTransaction = ReactiveCommand.CreateFromTask<SmsMessage,SmsMessage>(async sms =>
            {
                sms.CompletedTime = DateTime.UtcNow;
                sms.TransactionId = Guid.NewGuid();
                Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<SmsMessage> created = _dataContext.SmsMessages.Add(sms);
                await _dataContext.SaveChangesAsync();
                sms.Id = created.Entity.Id;
                var mobileServer = await _dataContext.MobileServers.FirstAsync(x => x.Id == sms.MobileServerId);
                sms.MobileServer = mobileServer;
                return sms;
            });
            SendSmsToMobileServer = ReactiveCommand.CreateFromTask<SmsMessage,Unit>(async sms =>
            {
                var serverKey = sms.MobileServerId.ToString();
                LatestTransaction.SimSlot = SelectedSimCard.SimSlot;
                LatestTransaction.MessageId = sms.Id;
                LatestTransaction.AppId = sms.AppId;
                LatestTransaction.Body = sms.Body;
                LatestTransaction.MobileServerId = sms.MobileServerId;
                LatestTransaction.ReceiverNumber = sms.RecieverNumber;
                LatestTransaction.SenderNumber = sms.SenderNumber;
                LatestTransaction.MessageStatus = sms.MessageStatus;
                LatestTransaction.TransactionId = sms.TransactionId;
                var serverConnectionId = string.Empty;
                if(_serverConnectionService.ConnectedServers.TryGetValue(serverKey, out serverConnectionId))
                    await _hubContext.Clients.Client(serverConnectionId).SendAsync(SignalRKeys.SendSms, LatestTransaction);
                return Unit.Default;

            });
            AddSmsTransaction.InvokeCommand(SendSmsToMobileServer);
            AddSmsTransaction.Do(transaction => SmsTransactions.Add(transaction)).Subscribe();

            SendSmsToMobileServer.ThrownExceptions.Select(x => x.Message).ToPropertyEx(this, x => x.Errors);
            AddSmsTransaction.ThrownExceptions.Select(x => x.Message).ToPropertyEx(this, x => x.Errors);
            LoadSimCards.ThrownExceptions.Select(x => x.Message).ToPropertyEx(this, x => x.Errors);

            _hubEventService.OnMessageStateChanged.ObserveOn(RxApp.MainThreadScheduler).Subscribe(sms =>
            {
                var item = SmsTransactions.FirstOrDefault(x => x.Id == sms.Id);
                if(item != null)
                {
                    item.CompletedTime = sms.CompletedTime;
                    item.MessageStatus = sms.MessageStatus;
                    SmsTransactions.Remove(item);
                    SmsTransactions.Add(item);
                    SmsTransactions = new ObservableCollection<SmsMessage>(SmsTransactions.OrderByDescending(x => x.CompletedTime));
                }
            });
        }

        [Reactive]
        public ObservableCollection<MobileServer> MobileServers { get; set; }

        [Reactive]
        public ObservableCollection<SmsMessage> SmsTransactions { get; set; }

        [Reactive]
        public ObservableCollection<SimCard> Sims { get; set; }

        public string? Errors { [ObservableAsProperty]get; }

        public SimCard SelectedSimCard { get; set; }

        public SmsRequest LatestTransaction { get; set; } = new SmsRequest();

        public ReactiveCommand<Unit,List<MobileServer>> LoadMobileServers { get; }

        public ReactiveCommand<Unit, List<SimCard>> LoadSimCards { get; }

        public ReactiveCommand<SmsMessage,SmsMessage> AddSmsTransaction { get; }

        public ReactiveCommand<SmsMessage,Unit> SendSmsToMobileServer { get; }
    }
}
