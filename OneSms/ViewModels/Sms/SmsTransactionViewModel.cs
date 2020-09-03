using Microsoft.EntityFrameworkCore;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Linq;
using OneSms.Data;
using OneSms.Domain;
using System.Threading.Tasks;
using OneSms.Services;

namespace OneSms.ViewModels
{
    public class SmsTransactionViewModel:ReactiveObject
    {
        private readonly DataContext _dbContext;
        private readonly ISmsService _smsService;

        public SmsTransactionViewModel(DataContext dbContext,ISmsService smsService)
        {
            _dbContext = dbContext;
            _smsService = smsService;

            LoadApps = ReactiveCommand.CreateFromTask<string, List<Application>>(userId => string.IsNullOrEmpty(userId) ? _dbContext.Apps.ToListAsync() : _dbContext.Apps.Where(x => x.UserId == userId).ToListAsync());
            LoadApps.Do(apps => SelectedApp = apps.FirstOrDefault()).Subscribe();
            LoadApps.Do(apps => Apps = new ObservableCollection<Application>(apps)).Subscribe();

            Transactions = new ObservableCollection<SmsMessage>();
            
            LoadSmsTransactions = ReactiveCommand.CreateFromTask<Application>(app => LoadSmsMessages(app));
            LoadSmsTransactions.Do(_ => Transactions = new ObservableCollection<SmsMessage>(Transactions)).Subscribe();

            this.WhenAnyValue(x => x.SelectedApp).Where(x => x != null).DistinctUntilChanged().InvokeCommand(LoadSmsTransactions);

            DeleteTransaction = ReactiveCommand.CreateFromTask<SmsMessage, int>(sms =>
             {
                 _dbContext.Remove(sms);
                 return _dbContext.SaveChangesAsync();
             });
            DeleteTransaction.Select(_ => SelectedApp).InvokeCommand(LoadSmsTransactions);
        }
        

        [Reactive]
        public ObservableCollection<SmsMessage> Transactions { get; set; }

        [Reactive]
        public ObservableCollection<Application> Apps { get; set; } = new ObservableCollection<Application>();

        [Reactive]
        public Application SelectedApp { get; set; } = null!;

        public ReactiveCommand<Application,Unit> LoadSmsTransactions { get; }

        public ReactiveCommand<string, List<Application>> LoadApps { get; }

        public ReactiveCommand<SmsMessage,int> DeleteTransaction { get; }

        private async Task LoadSmsMessages(Application application)
        {
            Transactions.Clear();
            await foreach(var message in _smsService.GetMessages(application.Id))
            {
                Transactions.Add(message);
            }
        }
    }
}
