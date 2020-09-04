using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using OneSms.Data;
using OneSms.Hubs;
using OneSms.Services;
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
    public class UssdTransactionViewModel : ReactiveObject
    {
        private OneSmsDbContext _oneSmsDbContext;
        private IHubContext<OneSmsHub> _oneSmsHubContext;
        private HubEventService _smsHubEventService;

        public UssdTransactionViewModel(OneSmsDbContext oneSmsDbContext, IHubContext<OneSmsHub> oneSmsHubContext, HubEventService smsHubEventService)
        {
            _oneSmsDbContext = oneSmsDbContext;
            _oneSmsHubContext = oneSmsHubContext;
            _smsHubEventService = smsHubEventService;
            Transactions = new ObservableCollection<UssdTransaction>();
            LoadTransactions = ReactiveCommand.CreateFromTask(() => _oneSmsDbContext.UssdTransactions.Include(x => x.Sim).Include(x => x.MobileServer).OrderByDescending(x => x.CompletedTime).ToListAsync());
            LoadTransactions.Do(ussd => Transactions = new ObservableCollection<UssdTransaction>(ussd)).Subscribe();

            DeleteTransaction = ReactiveCommand.CreateFromTask<UssdTransaction, int>(ussd =>
            {
                _oneSmsDbContext.Remove(ussd);
                return _oneSmsDbContext.SaveChangesAsync();
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
