using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using OneSms.Data;
using OneSms.Hubs;
using OneSms.Services;
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

namespace OneSms.ViewModels
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

            LoadTransactions = ReactiveCommand.CreateFromTask<Unit, List<TimTransaction>>(_ => _oneSmsDbContext.TimTransactions.Include(x => x.Client).OrderByDescending(x => x.EndTime).ToListAsync());
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
                transaction.StartTime = DateTime.UtcNow;
                transaction.EndTime = DateTime.UtcNow;
                Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<TimTransaction> created = _oneSmsDbContext.TimTransactions.Add(transaction);
                await _oneSmsDbContext.SaveChangesAsync();
                return transaction;
            });
            AddOrUpdate.Select(_ => Unit.Default).InvokeCommand(LoadTransactions);
            AddOrUpdate.ThrownExceptions.Select(x => $"Message: {x.Message}, Stack Trace:{x.StackTrace}").ToPropertyEx(this, x => x.Errors);

            SendTransaction = ReactiveCommand.CreateFromTask<TimTransaction, Unit>(async transaction =>
             {
                 var ussdAction = await _oneSmsDbContext.UssdActions.Include(x => x.Steps).FirstOrDefaultAsync(x => x.ActionType == UssdActionType.TimTransaction);
                 var mobileServer = await _oneSmsDbContext.MobileServers.Include(x => x.Sims).FirstOrDefaultAsync(x => x.IsTimServer);
                 var sims = mobileServer?.Sims;

                 if(ussdAction != null && mobileServer != null)
                 {
                     var selectedSim = sims.FirstOrDefault(x => int.Parse(x.AirtimeBalance ?? "0") > 0) ?? sims.FirstOrDefault();
                     var inputs = GetInputs(transaction, ussdAction);
                     var ussd = new UssdTransactionDto()
                     {
                         ActionType = UssdActionType.TimTransaction,
                         ClientId = transaction.ClientId ?? 0,
                         IsTimTransaction = true,
                         KeyProblems = ussdAction.KeyProblems.Split(",").ToList(),
                         KeyWelcomes = ussdAction.KeyLogins.Split(",").ToList(),
                         UssdNumber = ussdAction.UssdNumber,
                         TimeStamp = DateTime.UtcNow,
                         UssdTransactionId = transaction.Id,
                         SimId = selectedSim.Id,
                         SimSlot = selectedSim.SimSlot,
                         TransactionState = UssdTransactionState.Sent,
                         UssdInputs = inputs
                     };
                     var serverConnectionId = string.Empty;
                     if (_serverConnectionService.ConnectedServers.TryGetValue(mobileServer.Key.ToString(), out serverConnectionId))
                         await _oneSmsHubContext.Clients.Client(serverConnectionId).SendAsync(SignalRKeys.SendUssd, ussd);
                 }
                 return Unit.Default;
             });
            AddOrUpdate.InvokeCommand(SendTransaction);
            SendTransaction.ThrownExceptions.Select(x => $"Message: {x.Message}, Stack Trace:{x.StackTrace}").ToPropertyEx(this, x => x.Errors);

            _hubEventService.OnUssdStateChanged.ObserveOn(RxApp.MainThreadScheduler).Subscribe(ussd =>
            {
                var item = Transactions.FirstOrDefault();
                if (item != null)
                {
                    item.EndTime = ussd.EndTime;
                    item.TransactionState = ussd.TransactionState;
                    item.LastMessage = ussd.LastMessage;
                    Transactions.Remove(item);
                    Transactions.Add(item);
                    Transactions = new ObservableCollection<TimTransaction>(Transactions.OrderByDescending(x => x.EndTime));
                }
            });
        }

        private List<string> GetInputs(TimTransaction transaction, UssdAction ussdAction)
        {
            var inputs = new List<string>();
            var placeHolderData = new Queue<string>();
            placeHolderData.Enqueue(transaction.Number);
            placeHolderData.Enqueue(GetMinuteIndex(int.Parse(transaction.Minutes)).ToString());
            foreach (var step in ussdAction.Steps)
            {
                if (step.IsPlaceHolder)
                    step.Value = placeHolderData.Dequeue();
                inputs.Add(step.Value);
            }
            return inputs;
        }

        private int GetMinuteIndex(int minutes)
        {
            if (minutes >= 0 && minutes < 3)
                return 5;
            if (minutes >= 3 && minutes < 8)
                return 4;
            if (minutes >= 8 && minutes < 16)
                return 2;
            return 1;
        }

        public string? Errors { [ObservableAsProperty]get; }

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
