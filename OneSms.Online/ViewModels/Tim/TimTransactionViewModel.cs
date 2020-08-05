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
    public class TimTransactionViewModel:ReactiveObject
    {
        private OneSmsDbContext _oneSmsDbContext;
        private IHubContext<OneSmsHub> _oneSmsHubContext;
        private HubEventService _hubEventService;
        private ServerConnectionService _serverConnectionService;

        public TimTransactionViewModel(OneSmsDbContext oneSmsDbContext, IHubContext<OneSmsHub> oneSmsHubContext, HubEventService hubEventService, ServerConnectionService serverConnectionService)
        {
            _oneSmsDbContext = oneSmsDbContext;
            _oneSmsHubContext = oneSmsHubContext;
            _hubEventService = hubEventService;
            _serverConnectionService = serverConnectionService;

            LoadTransactions = ReactiveCommand.CreateFromTask<Unit, List<TimTransaction>>(_ => _oneSmsDbContext.TimTransactions.Include(x => x.Client).ToListAsync());
            LoadTransactions.Do(transactions => Transactions = new ObservableCollection<TimTransaction>(transactions)).Subscribe();
            LoadTransactions.ThrownExceptions.Select(x => x.Message).ToPropertyEx(this, x => x.Errors);

            Delete = ReactiveCommand.CreateFromTask<TimTransaction, int>(transaction =>
            {
                _oneSmsDbContext.Remove(transaction);
                return _oneSmsDbContext.SaveChangesAsync();
            });
            Delete.Where(rows => rows > 0).Select(_ => Unit.Default).InvokeCommand(LoadTransactions);
            Delete.ThrownExceptions.Select(x => x.Message).ToPropertyEx(this, x => x.Errors);

            AddOrUpdate = ReactiveCommand.CreateFromTask<TimTransaction, TimTransaction>(async transaction =>
            {
                Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<TimTransaction> created = _oneSmsDbContext.TimTransactions.Add(transaction);
                await _oneSmsDbContext.SaveChangesAsync();
                return transaction;
            });
            AddOrUpdate.Select(_ => Unit.Default).InvokeCommand(LoadTransactions);
            AddOrUpdate.ThrownExceptions.Select(x => x.Message).ToPropertyEx(this, x => x.Errors);

            SendTransaction = ReactiveCommand.CreateFromTask<TimTransaction, Unit>(async transaction =>
             {
                 var ussdAction = _oneSmsDbContext.UssdActions.FirstOrDefault(x => x.ActionType == UssdActionType.TimTransaction);
                 var mobileServer = _oneSmsDbContext.MobileServers.Include(x => x.Sims).FirstOrDefault(x => x.IsTimServer);
                 var sims = mobileServer?.Sims;

                 if(ussdAction != null && mobileServer != null)
                 {
                     var selectedSim = sims.FirstOrDefault(x => int.Parse(x.AirtimeBalance) > 0) ?? sims.FirstOrDefault();
                     var ussd = new UssdTransactionDto()
                     {
                         ActionType = UssdActionType.TimTransaction,
                         ClientId = (int)transaction.ClientId,
                         IsTimTransaction = true,
                         KeyProblems = ussdAction.KeyProblems.Split(",").ToList(),
                         KeyWelcomes = ussdAction.KeyLogins.Split(",").ToList(),
                         UssdNumber = ussdAction.UssdNumber,
                         TimeStamp = DateTime.UtcNow,
                         UssdTransactionId = transaction.Id,
                         SimId = selectedSim.Id,
                         SimSlot = selectedSim.SimSlot
                     };
                     var serverConnectionId = string.Empty;
                     if (_serverConnectionService.ConnectedServers.TryGetValue(mobileServer.Key.ToString(), out serverConnectionId))
                         await _oneSmsHubContext.Clients.Client(serverConnectionId).SendAsync(SignalRKeys.SendUssd, ussd);
                 }
                 return Unit.Default;
             });
            AddOrUpdate.InvokeCommand(SendTransaction);
            SendTransaction.ThrownExceptions.Select(x => x.Message).ToPropertyEx(this, x => x.Errors);

            _hubEventService.OnUssdStateChanged.ObserveOn(RxApp.MainThreadScheduler).Subscribe(ussd =>
            {
                var item = Transactions.FirstOrDefault(x => x.Id == ussd.UssdTransactionId);
                if (item != null)
                {
                    item.EndTime = ussd.TimeStamp;
                    item.TransactionState = ussd.TransactionState;
                    Transactions.Remove(item);
                    Transactions.Add(item);
                    Transactions = new ObservableCollection<TimTransaction>(Transactions.OrderByDescending(x => x.EndTime));
                }
            });
        }

        public string Errors { [ObservableAsProperty]get; }

        [Reactive]
        public bool IsBusy { get; set; }

        [Reactive]
        public ObservableCollection<TimTransaction> Transactions { get; set; } = new ObservableCollection<TimTransaction>();

        public ReactiveCommand<Unit, List<TimTransaction>> LoadTransactions { get; }

        public ReactiveCommand<TimTransaction, TimTransaction> AddOrUpdate { get; }

        public ReactiveCommand<TimTransaction, int> Delete { get; }

        public ReactiveCommand<TimTransaction, Unit> SendTransaction { get; }
    }
}
