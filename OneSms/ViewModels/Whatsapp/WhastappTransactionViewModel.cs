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
using System.Threading.Tasks;

namespace OneSms.ViewModels.Whatsapp
{
    public class WhastappTransactionViewModel:ReactiveObject
    {
        private readonly DataContext _dbContext;
        private readonly IWhatsappService _whatsappService;

        public WhastappTransactionViewModel(DataContext dbContext,IWhatsappService whatsappService)
        {
            _dbContext = dbContext;
            _whatsappService = whatsappService;

            LoadApps = ReactiveCommand.CreateFromTask<string, List<Application>>(userId => string.IsNullOrEmpty(userId) ? _dbContext.Apps.ToListAsync() : _dbContext.Apps.Where(x => x.UserId == userId).ToListAsync());
            LoadApps.Do(apps => SelectedApp = apps.FirstOrDefault()).Subscribe();
            LoadApps.Do(apps => Apps = new ObservableCollection<Application>(apps)).Subscribe();

            LoadTransactions = ReactiveCommand.CreateFromTask<Application>(app => LoadMessages(app));
            LoadTransactions.Do(_ => Transactions = new ObservableCollection<WhatsappMessage>(Transactions)).Subscribe();
            LoadTransactions.ThrownExceptions.Select(x => x.Message).ToPropertyEx(this, x => x.Errors);

            this.WhenAnyValue(x => x.SelectedApp).Where(x => x != null).DistinctUntilChanged().InvokeCommand(LoadTransactions);
            

            DeleteTransaction = ReactiveCommand.CreateFromTask<WhatsappMessage, int>(whatsapp =>
            {
                _dbContext.Remove(whatsapp);
                return _dbContext.SaveChangesAsync();
            });
            DeleteTransaction.Select(_ => SelectedApp).InvokeCommand(LoadTransactions);
        }

        [Reactive]
        public ObservableCollection<WhatsappMessage> Transactions { get; set; } = new ObservableCollection<WhatsappMessage>();

        [Reactive]
        public ObservableCollection<Application> Apps { get; set; } = new ObservableCollection<Application>();

        [Reactive]
        public Application SelectedApp { get; set; } = null!;

        public string? Errors { [ObservableAsProperty]get; }

        public ReactiveCommand<Application, Unit> LoadTransactions { get; }

        public ReactiveCommand<WhatsappMessage, int> DeleteTransaction { get; }

        public ReactiveCommand<string, List<Application>> LoadApps { get; }

        private async Task LoadMessages(Application application)
        {
            Transactions.Clear();
            await foreach (var message in _whatsappService.GetMessages(application.Id))
            {
                Transactions.Add(message);
            }
        }
    }
}
