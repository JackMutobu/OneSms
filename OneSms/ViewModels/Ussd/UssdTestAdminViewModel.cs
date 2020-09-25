using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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
    public class UssdTestAdminViewModel:ReactiveObject
    {
        private readonly DataContext _dbContext;
        private readonly HubEventService _hubEventService;
        private readonly ServerConnectionService _serverConnectionService;
        private readonly IHubContext<OneSmsHub> _hubContext;

        public UssdTestAdminViewModel(DataContext dbContext,HubEventService hubEventService, ServerConnectionService serverConnectionService, IHubContext<OneSmsHub> hubContext)
        {
            _dbContext = dbContext;
            _hubEventService = hubEventService;
            _serverConnectionService = serverConnectionService;
            _hubContext = hubContext;
            UssdTransactions = new ObservableCollection<UssdTransaction>();
            SimCards = new ObservableCollection<SimCard>();
            MobileServers = new ObservableCollection<MobileServer>();
            UssdActions = new ObservableCollection<UssdAction>(_dbContext.UssdActions.Include(x => x.Steps));
            SelectedAction = UssdActions.FirstOrDefault();
            LoadSimCards = ReactiveCommand.CreateFromTask(() => _dbContext.Sims.Include(x => x.MobileServer).ToListAsync());
            LoadSimCards.Do(sims => SimCards = new ObservableCollection<SimCard>(sims)).Subscribe();
            LoadSimCards.Do(sims => SelectedSimCard = sims.FirstOrDefault());

            AddUssdTransaction = ReactiveCommand.CreateFromTask<UssdTransaction, (UssdRequest request, UssdTransaction transaction)>(async transaction =>
             {
                 EntityEntry<UssdTransaction> created = _dbContext.UssdTransactions.Add(transaction);
                 transaction.Id = created.Entity.Id;
                 await _dbContext.SaveChangesAsync();
                 var request =  new UssdRequest
                 {
                     SimId = transaction.SimId,
                     KeyProblems = SelectedAction!.KeyProblems.Split(',').ToList(),
                     KeyWelcomes = SelectedAction.KeyProblems.Split(',').ToList(),
                     NetworkAction = SelectedAction.ActionType,
                     SimSlot = SelectedSimCard!.SimSlot,
                     TransactionId = transaction.TransactionId,
                     UssdId = transaction.Id,
                     UssdNumber = SelectedAction.UssdNumber,
                     UssdInputs = SelectedAction.Steps.OrderBy(x => x.Id).Select(x => x.Value).ToList(),
                     MobileServerId = SelectedSimCard.MobileServerId
                 };
                 return (request, transaction);
             });
            AddUssdTransaction.Do(trans => UssdTransactions.Add(trans.transaction)).Subscribe();
            AddUssdTransaction.ThrownExceptions.Select(x => x.Message).ToPropertyEx(this, x => x.Errors);

            SendUssdTransaction = ReactiveCommand.CreateFromTask<UssdRequest, Unit>(async ussd =>
             {
                 var serverKey = SelectedSimCard!.MobileServerId.ToString();
                 var serverConnectionId = string.Empty;
                 if (_serverConnectionService.ConnectedServers.TryGetValue(serverKey, out serverConnectionId))
                     await _hubContext.Clients.Client(serverConnectionId).SendAsync(SignalRKeys.SendUssd, ussd);
                 CurrentServerKey = serverKey;
                 return Unit.Default;
             });
            AddUssdTransaction.Select(x => x.request).InvokeCommand(SendUssdTransaction);
            SendUssdTransaction.ThrownExceptions.Select(x => x.Message).ToPropertyEx(this, x => x.Errors);

            _hubEventService.OnUssdStateChanged.ObserveOn(RxApp.MainThreadScheduler).Subscribe(ussd =>
            {
                var item = UssdTransactions.FirstOrDefault(x => x.Id == ussd.Id);
                if (item != null)
                {
                    item.EndTime = item.EndTime;
                    item.TransactionState = ussd.TransactionState;
                    item.LastMessage = ussd.LastMessage;
                    UssdTransactions.Remove(item);
                    UssdTransactions.Add(item);
                    UssdTransactions = new ObservableCollection<UssdTransaction>(UssdTransactions.OrderByDescending(x => x.EndTime));
                }
            });

            LoadMobileServers = ReactiveCommand.CreateFromTask(() => _dbContext.MobileServers.ToListAsync());
            LoadMobileServers.Do(servers => MobileServers = new ObservableCollection<MobileServer>(servers));
            LoadMobileServers.Select(_ => Unit.Default).InvokeCommand(LoadSimCards);
            CancelUssdSession = ReactiveCommand.CreateFromTask<string,Unit>(async key =>
            {
                if(!string.IsNullOrEmpty(key))
                {
                    var serverConnectionId = string.Empty;
                    if (_serverConnectionService.ConnectedServers.TryGetValue(key, out serverConnectionId))
                        await _hubContext.Clients.Client(serverConnectionId).SendAsync(SignalRKeys.CancelUssdSession);
                }
                return Unit.Default;
            });
        }
        public SimCard? SelectedSimCard { get; set; }

        public UssdAction? SelectedAction { get; set; }

        public string? Errors { [ObservableAsProperty]get; }

        [Reactive]
        public string? CurrentServerKey { get; set; }

        [Reactive]
        public ObservableCollection<UssdAction> UssdActions { get; set; } 

        [Reactive]
        public ObservableCollection<UssdTransaction> UssdTransactions { get; set; } 

        [Reactive]
        public ObservableCollection<SimCard> SimCards { get; set; }

        [Reactive]
        public ObservableCollection<MobileServer> MobileServers { get; set; }

        public ReactiveCommand<Unit,List<SimCard>> LoadSimCards { get; }

        public ReactiveCommand<Unit, List<MobileServer>> LoadMobileServers { get; }

        public ReactiveCommand<UssdTransaction, (UssdRequest request, UssdTransaction transaction)> AddUssdTransaction { get; }

        public ReactiveCommand<UssdRequest, Unit> SendUssdTransaction { get; }

        public ReactiveCommand<string,Unit> CancelUssdSession { get; }


    }
}
