using Microsoft.EntityFrameworkCore;
using OneSms.Data;
using OneSms.Domain;
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
    public class NetworkAdminViewModel:ReactiveObject
    {
        private DataContext _dbContext;

        public NetworkAdminViewModel(DataContext dbContext)
        {
            _dbContext = dbContext;
            LoadNetworks = ReactiveCommand.CreateFromTask(() => _dbContext.Networks.ToListAsync());
            LoadNetworks.Do(networks => Networks = new ObservableCollection<NetworkOperator>(networks)).Subscribe();
            AddOrUpdateNetwork = ReactiveCommand.CreateFromTask<NetworkOperator, int>(network =>
             {
                 _dbContext.Update(network);
                 return _dbContext.SaveChangesAsync();
             });
            AddOrUpdateNetwork.Where(rows => rows > 0).Select(_ => Unit.Default).InvokeCommand(LoadNetworks);
            DeleteNetwork = ReactiveCommand.CreateFromTask<NetworkOperator, int>(network =>
            {
                _dbContext.Remove(network);
                return _dbContext.SaveChangesAsync();
            });
            DeleteNetwork.Where(rows => rows > 0).Select(_ => Unit.Default).InvokeCommand(LoadNetworks);
        }

        [Reactive]
        public bool IsBusy { get; set; }

        [Reactive]
        public ObservableCollection<NetworkOperator> Networks { get; set; } = new ObservableCollection<NetworkOperator>();

        public ReactiveCommand<Unit,List<NetworkOperator>> LoadNetworks { get; }

        public ReactiveCommand<NetworkOperator, int> AddOrUpdateNetwork { get; }

        public ReactiveCommand<NetworkOperator, int> DeleteNetwork { get; }
    }
}
