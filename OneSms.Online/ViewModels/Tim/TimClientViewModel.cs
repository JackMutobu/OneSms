using Microsoft.EntityFrameworkCore;
using OneSms.Online.Data;
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
    public class TimClientViewModel:ReactiveObject
    {
        private OneSmsDbContext _oneSmsDbContext;

        public TimClientViewModel(OneSmsDbContext oneSmsDbContext)
        {
            _oneSmsDbContext = oneSmsDbContext;
            LoadClients = ReactiveCommand.CreateFromTask(() => _oneSmsDbContext.TimClients.ToListAsync());
            LoadClients.Do(clients => Clients = new ObservableCollection<TimClient>(clients)).Subscribe();
            LoadClients.ThrownExceptions.Select(x => x.Message).ToPropertyEx(this, x => x.Errors);

            AddOrUpdate = ReactiveCommand.CreateFromTask<TimClient, int>(client =>
            {
                _oneSmsDbContext.Update(client);
                return _oneSmsDbContext.SaveChangesAsync();
            });
            AddOrUpdate.Where(rows => rows > 0).Select(_ => Unit.Default).InvokeCommand(LoadClients);
            AddOrUpdate.ThrownExceptions.Select(x => x.Message).ToPropertyEx(this, x => x.Errors);

            Delete = ReactiveCommand.CreateFromTask<TimClient, int>(client =>
            {
                _oneSmsDbContext.Remove(client);
                return _oneSmsDbContext.SaveChangesAsync();
            });
            Delete.Where(rows => rows > 0).Select(_ => Unit.Default).InvokeCommand(LoadClients);
            Delete.ThrownExceptions.Select(x => x.Message).ToPropertyEx(this, x => x.Errors);

            LoadTransactions = ReactiveCommand.CreateFromTask<TimClient, List<TimTransaction>>(client => _oneSmsDbContext.TimTransactions.Where(x => x.ClientId == client.Id).ToListAsync());
            LoadTransactions.Do(transactions => Transactions = new ObservableCollection<TimTransaction>(transactions)).Subscribe();
        }

        public string Errors { [ObservableAsProperty]get; }

        [Reactive]
        public bool IsBusy { get; set; }

        [Reactive]
        public ObservableCollection<TimClient> Clients { get; set; } = new ObservableCollection<TimClient>();

        [Reactive]
        public ObservableCollection<TimTransaction> Transactions { get; set; } = new ObservableCollection<TimTransaction>();

        public ReactiveCommand<Unit, List<TimClient>> LoadClients { get; }

        public ReactiveCommand<TimClient, List<TimTransaction>> LoadTransactions { get; }

        public ReactiveCommand<TimClient, int> AddOrUpdate { get; }

        public ReactiveCommand<TimClient, int> Delete { get; }
    }
}
