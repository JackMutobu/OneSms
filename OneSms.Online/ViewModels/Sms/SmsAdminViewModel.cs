using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using OneSms.Online.Data;
using OneSms.Online.Hubs;
using OneSms.Online.Services;
using OneSms.Web.Shared.Constants;
using OneSms.Web.Shared.Dtos;
using OneSms.Web.Shared.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace OneSms.Online.ViewModels
{
    public class SmsAdminViewModel:ReactiveObject
    {
        private OneSmsDbContext _oneSmsDbContext;
        private ServerConnectionService _serverConnectionService;
        private IHubContext<OneSmsHub> _oneSmsHubContext;
        private HubEventService _smsHubEventService;

        public SmsAdminViewModel(OneSmsDbContext oneSmsDbContext,ServerConnectionService serverConnectionService,IHubContext<OneSmsHub> oneSmsHubContext,HubEventService smsHubEventService)
        {
            _oneSmsDbContext = oneSmsDbContext;
            _serverConnectionService = serverConnectionService;
            _oneSmsHubContext = oneSmsHubContext;
            _smsHubEventService = smsHubEventService;
            SmsTransactions = new ObservableCollection<SmsTransaction>();
            Sims = new ObservableCollection<SimCard>();

            LoadMobileServers = ReactiveCommand.CreateFromTask(() => _oneSmsDbContext.MobileServers.ToListAsync());

            LoadSimCards = ReactiveCommand.CreateFromTask(() => _oneSmsDbContext.Sims.Include(x => x.MobileServer).Include(x => x.Apps).ToListAsync());
            LoadSimCards.Do(sims => Sims = new ObservableCollection<SimCard>(sims)).Subscribe();
            AddSmsTransaction = ReactiveCommand.CreateFromTask<SmsTransaction,SmsTransaction>(async sms =>
            {
                sms.CreatedOn = DateTime.UtcNow;
                sms.CompletedTime = DateTime.UtcNow;
                sms.TransactionId = Guid.NewGuid();
                Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<SmsTransaction> created = _oneSmsDbContext.SmsTransactions.Add(sms);
                await _oneSmsDbContext.SaveChangesAsync();
                sms.Id = created.Entity.Id;
                var mobileServer = await _oneSmsDbContext.MobileServers.FirstAsync(x => x.Id == sms.MobileServerId);
                sms.MobileServer = mobileServer;
                return sms;
            });
            SendSmsToMobileServer = ReactiveCommand.CreateFromTask<SmsTransaction,Unit>(async sms =>
            {
                var serverKey = sms.MobileServer.Key.ToString();
                LatestTransaction.SimSlot = SelectedSimCard.SimSlot;
                LatestTransaction.SmsId = sms.Id;
                LatestTransaction.AppId = sms.OneSmsAppId;
                LatestTransaction.Message = sms.Body;
                LatestTransaction.MessageTransactionProcessor = sms.MessageTransactionProcessor;
                LatestTransaction.MobileServerId = sms.MobileServerId;
                LatestTransaction.ReceiverNumber = sms.RecieverNumber;
                LatestTransaction.SenderNumber = sms.SenderNumber;
                LatestTransaction.TimeStamp = sms.StartTime;
                LatestTransaction.TransactionState = sms.TransactionState;
                LatestTransaction.TransactionId = sms.TransactionId;
                var serverConnectionId = string.Empty;
                if(_serverConnectionService.ConnectedServers.TryGetValue(serverKey, out serverConnectionId))
                    await _oneSmsHubContext.Clients.Client(serverConnectionId).SendAsync(SignalRKeys.SendSms, LatestTransaction);
                return Unit.Default;

            });
            AddSmsTransaction.InvokeCommand(SendSmsToMobileServer);
            AddSmsTransaction.Do(transaction => SmsTransactions.Add(transaction)).Subscribe();

            SendSmsToMobileServer.ThrownExceptions.Select(x => x.Message).ToPropertyEx(this, x => x.Errors);
            AddSmsTransaction.ThrownExceptions.Select(x => x.Message).ToPropertyEx(this, x => x.Errors);
            LoadSimCards.ThrownExceptions.Select(x => x.Message).ToPropertyEx(this, x => x.Errors);

            _smsHubEventService.OnMessageStateChanged.ObserveOn(RxApp.MainThreadScheduler).Subscribe(sms =>
            {
                var item = SmsTransactions.FirstOrDefault(x => x.Id == sms.SmsId);
                if(item != null)
                {
                    item.CompletedTime = sms.TimeStamp;
                    item.TransactionState = sms.TransactionState;
                    SmsTransactions.Remove(item);
                    SmsTransactions.Add(item);
                    SmsTransactions = new ObservableCollection<SmsTransaction>(SmsTransactions.OrderByDescending(x => x.CompletedTime));
                }
            });
        }

        [Reactive]
        public ObservableCollection<ServerMobile> MobileServers { get; set; }

        [Reactive]
        public ObservableCollection<SmsTransaction> SmsTransactions { get; set; }

        [Reactive]
        public ObservableCollection<SimCard> Sims { get; set; }

        public string Errors { [ObservableAsProperty]get; }

        public SimCard SelectedSimCard { get; set; }

        public MessageTransactionProcessDto LatestTransaction { get; set; }

        public ReactiveCommand<Unit,List<ServerMobile>> LoadMobileServers { get; }

        public ReactiveCommand<Unit, List<SimCard>> LoadSimCards { get; }

        public ReactiveCommand<SmsTransaction,SmsTransaction> AddSmsTransaction { get; }

        public ReactiveCommand<SmsTransaction,Unit> SendSmsToMobileServer { get; }
    }
}
