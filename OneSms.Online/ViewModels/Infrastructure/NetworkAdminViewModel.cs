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
    public class NetworkAdminViewModel:ReactiveObject
    {
        private OneSmsDbContext _oneSmsDbContext;

        public NetworkAdminViewModel(OneSmsDbContext oneSmsDbContext)
        {
            _oneSmsDbContext = oneSmsDbContext;
            LoadNetworks = ReactiveCommand.CreateFromTask(() => _oneSmsDbContext.Networks.ToListAsync());
            LoadNetworks.Do(networks => Networks = new ObservableCollection<NetworkOperator>(networks)).Subscribe();
            AddOrUpdateNetwork = ReactiveCommand.CreateFromTask<NetworkOperator, int>(network =>
             {
                 network.CreatedOn = DateTime.UtcNow;
                 _oneSmsDbContext.Update(network);
                 return _oneSmsDbContext.SaveChangesAsync();
             });
            AddOrUpdateNetwork.Where(rows => rows > 0).Select(_ => Unit.Default).InvokeCommand(LoadNetworks);
            DeleteNetwork = ReactiveCommand.CreateFromTask<NetworkOperator, int>(network =>
            {
                _oneSmsDbContext.Remove(network);
                return _oneSmsDbContext.SaveChangesAsync();
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
