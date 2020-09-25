using Microsoft.EntityFrameworkCore;
using OneSms.Data;
using OneSms.Domain;
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
    public class UssdTransactionViewModel : ReactiveObject
    {
        private DataContext _dbContext;
        private HubEventService _smsHubEventService;

        public UssdTransactionViewModel(DataContext dbContext, HubEventService smsHubEventService)
        {
            _dbContext = dbContext;
            _smsHubEventService = smsHubEventService;
            Transactions = new ObservableCollection<UssdTransaction>();
            LoadTransactions = ReactiveCommand.CreateFromTask(() => _dbContext.UssdTransactions.Include(x => x.Sim).Include(x => x.UssdAction).OrderByDescending(x => x.EndTime).ToListAsync());
            LoadTransactions.Do(ussd => Transactions = new ObservableCollection<UssdTransaction>(ussd)).Subscribe();

            DeleteTransaction = ReactiveCommand.CreateFromTask<UssdTransaction, int>(ussd =>
            {
                _dbContext.Remove(ussd);
                return _dbContext.SaveChangesAsync();
            });
            DeleteTransaction.Select(_ => Unit.Default).InvokeCommand(LoadTransactions);
            _smsHubEventService.OnUssdStateChanged.Select(_ => Unit.Default).InvokeCommand(LoadTransactions);
        }

        [Reactive]
        public ObservableCollection<UssdTransaction> Transactions { get; set; }

        public ReactiveCommand<Unit, List<UssdTransaction>> LoadTransactions { get; }

        public ReactiveCommand<UssdTransaction, int> DeleteTransaction { get; }
    }
}
