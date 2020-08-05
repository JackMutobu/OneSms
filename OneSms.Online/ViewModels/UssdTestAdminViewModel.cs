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

namespace OneSms.Online.ViewModels
{
    public class UssdTestAdminViewModel:ReactiveObject
    {
        private readonly OneSmsDbContext _oneSmsDbContext;
        private readonly HubEventService _hubEventService;
        private readonly ServerConnectionService _serverConnectionService;
        private readonly IHubContext<OneSmsHub> _oneSmsHubContext;

        public UssdTestAdminViewModel(OneSmsDbContext oneSmsDbContext,HubEventService hubEventService, ServerConnectionService serverConnectionService, IHubContext<OneSmsHub> oneSmsHubContext)
        {
            _oneSmsDbContext = oneSmsDbContext;
            _hubEventService = hubEventService;
            _serverConnectionService = serverConnectionService;
            _oneSmsHubContext = oneSmsHubContext;
            UssdTransactions = new ObservableCollection<UssdTransaction>();
            SimCards = new ObservableCollection<SimCard>();
            MobileServers = new ObservableCollection<ServerMobile>();
            UssdActions = new ObservableCollection<UssdActionType>(Enum.GetValues(typeof(UssdActionType)).Cast<UssdActionType>());
            LoadSimCards = ReactiveCommand.CreateFromTask(() => _oneSmsDbContext.Sims.Include(x => x.MobileServer).ToListAsync());
            LoadSimCards.Do(sims => SimCards = new ObservableCollection<SimCard>(sims)).Subscribe();

            AddUssdTransaction = ReactiveCommand.CreateFromTask<UssdTransactionDto, UssdTransaction>(async transaction =>
             {
                 var ussdAction = _oneSmsDbContext.UssdActions.OrderByDescending(x => x.CreatedOn).Include(x => x.Steps).FirstOrDefault(x => x.ActionType == transaction.ActionType && x.NetworkId == SelectedSimCard.NetworkId);
                 var inputs = ussdAction.Steps.OrderBy(x => x.Id).Select(x => x.Value);
                 var transactionToAdd = new UssdTransaction(transaction)
                 {
                     MobileServerId = SelectedSimCard.MobileServerId
                 };
                 transactionToAdd.StartTime =  transactionToAdd.CompletedTime = transactionToAdd.CreatedOn = DateTime.UtcNow;
                 transaction.UssdInputs = new List<string>(inputs);
                 transaction.KeyProblems = ussdAction.KeyProblems.Split(",").ToList();
                 transaction.KeyWelcomes = ussdAction.KeyLogins.Split(",").ToList();

                 Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<UssdTransaction> created = _oneSmsDbContext.UssdTransactions.Add(transactionToAdd);
                 await _oneSmsDbContext.SaveChangesAsync();
                 transactionToAdd.Id = created.Entity.Id;
                 transaction.UssdTransactionId = created.Entity.Id;
                 transaction.UssdNumber = ussdAction.UssdNumber;
                 LatestTransaction = transaction;
                 return transactionToAdd;
             });
            AddUssdTransaction.Do(trans => UssdTransactions.Add(trans)).Subscribe();
            AddUssdTransaction.ThrownExceptions.Select(x => x.Message).ToPropertyEx(this, x => x.Errors);

            SendUssdTransaction = ReactiveCommand.CreateFromTask<UssdTransactionDto, Unit>(async ussd =>
             {
                 var serverKey = SelectedSimCard.MobileServer.Key.ToString();
                 var serverConnectionId = string.Empty;
                 if (_serverConnectionService.ConnectedServers.TryGetValue(serverKey, out serverConnectionId))
                     await _oneSmsHubContext.Clients.Client(serverConnectionId).SendAsync(SignalRKeys.SendUssd, ussd);
                 CurrentServerKey = serverKey;
                 return Unit.Default;
             });
            AddUssdTransaction.Select(_ => LatestTransaction).InvokeCommand(SendUssdTransaction);
            SendUssdTransaction.ThrownExceptions.Select(x => x.Message).ToPropertyEx(this, x => x.Errors);

            _hubEventService.OnUssdStateChanged.ObserveOn(RxApp.MainThreadScheduler).Subscribe(ussd =>
            {
                var item = UssdTransactions.FirstOrDefault(x => x.Id == ussd.UssdTransactionId);
                if (item != null)
                {
                    item.CompletedTime = ussd.TimeStamp;
                    item.TransactionState = ussd.TransactionState;
                    item.LastMessage = ussd.LastMessage;
                    UssdTransactions.Remove(item);
                    UssdTransactions.Add(item);
                    UssdTransactions = new ObservableCollection<UssdTransaction>(UssdTransactions.OrderByDescending(x => x.CompletedTime));
                }
            });

            LoadMobileServers = ReactiveCommand.CreateFromTask(() => _oneSmsDbContext.MobileServers.ToListAsync());
            LoadMobileServers.Do(servers => MobileServers = new ObservableCollection<ServerMobile>(servers));
            LoadMobileServers.Select(_ => Unit.Default).InvokeCommand(LoadSimCards);
            CancelUssdSession = ReactiveCommand.CreateFromTask<string,Unit>(async key =>
            {
                if(!string.IsNullOrEmpty(key))
                {
                    var serverConnectionId = string.Empty;
                    if (_serverConnectionService.ConnectedServers.TryGetValue(key, out serverConnectionId))
                        await _oneSmsHubContext.Clients.Client(serverConnectionId).SendAsync(SignalRKeys.CancelUssdSession);
                }
                return Unit.Default;
            });
        }
        public SimCard  SelectedSimCard { get; set; }

        public UssdActionType SelectedAction { get; set; }

        public UssdTransactionDto LatestTransaction { get; set; }

        public string Errors { [ObservableAsProperty]get; }

        [Reactive]
        public string CurrentServerKey { get; set; }

        [Reactive]
        public ObservableCollection<UssdActionType> UssdActions { get; set; } 

        [Reactive]
        public ObservableCollection<UssdTransaction> UssdTransactions { get; set; }

        [Reactive]
        public ObservableCollection<SimCard> SimCards { get; set; }

        [Reactive]
        public ObservableCollection<ServerMobile> MobileServers { get; set; }

        public ReactiveCommand<Unit,List<SimCard>> LoadSimCards { get; }

        public ReactiveCommand<Unit, List<ServerMobile>> LoadMobileServers { get; }

        public ReactiveCommand<UssdTransactionDto, UssdTransaction> AddUssdTransaction { get; }

        public ReactiveCommand<UssdTransactionDto, Unit> SendUssdTransaction { get; }

        public ReactiveCommand<string,Unit> CancelUssdSession { get; }


    }
}
