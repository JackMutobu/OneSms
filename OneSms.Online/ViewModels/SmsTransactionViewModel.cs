using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using OneSms.Online.Data;
using OneSms.Online.Hubs;
using OneSms.Web.Shared.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Linq;
using OneSms.Online.Services;

namespace OneSms.Online.ViewModels
{
    public class SmsTransactionViewModel:ReactiveObject
    {
        private OneSmsDbContext _oneSmsDbContext;
        private IHubContext<OneSmsHub> _oneSmsHubContext;
        private HubEventService _smsHubEventService;

        public SmsTransactionViewModel(OneSmsDbContext oneSmsDbContext,IHubContext<OneSmsHub> oneSmsHubContext,HubEventService smsHubEventService)
        {
            _oneSmsDbContext = oneSmsDbContext;
            _oneSmsHubContext = oneSmsHubContext;
            _smsHubEventService = smsHubEventService;
            SmsTransactions = new ObservableCollection<SmsTransaction>();
            LoadSmsTransactions = ReactiveCommand.CreateFromTask(() => _oneSmsDbContext.SmsTransactions.Include(x => x.MobileServer).OrderByDescending(x => x.CompletedTime).ToListAsync());
            LoadSmsTransactions.Do(sms => SmsTransactions = new ObservableCollection<SmsTransaction>(sms)).Subscribe();

            DeleteTransaction = ReactiveCommand.CreateFromTask<SmsTransaction, int>(sms =>
             {
                 _oneSmsDbContext.Remove(sms);
                 return _oneSmsDbContext.SaveChangesAsync();
             });
            DeleteTransaction.Select(_ => Unit.Default).InvokeCommand(LoadSmsTransactions);
            _smsHubEventService.OnSmsStateChanged.Select(_ => Unit.Default).InvokeCommand(LoadSmsTransactions);
        }

        [Reactive]
        public ObservableCollection<SmsTransaction> SmsTransactions { get; set; }

        public ReactiveCommand<Unit, List<SmsTransaction>> LoadSmsTransactions { get; }

        public ReactiveCommand<SmsTransaction,int> DeleteTransaction { get; }
    }
}
