using Microsoft.EntityFrameworkCore;
using OneSms.Data;
using OneSms.Domain;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;

namespace OneSms.ViewModels
{
    public class SimAdminViewModel:ReactiveObject
    {
        private DataContext _dbContext;

        public SimAdminViewModel(DataContext dbContext)
        {
            _dbContext = dbContext;
            Sims = new ObservableCollection<SimCard>();
            Networks = new ObservableCollection<NetworkOperator>();
            Apps = new ObservableCollection<Application>();

            LoadNetworks = ReactiveCommand.CreateFromTask(() => _dbContext.Networks.ToListAsync());
            LoadNetworks.Do(networks => Networks = new ObservableCollection<NetworkOperator>(networks)).Subscribe();

            LoadSimCards = ReactiveCommand.CreateFromTask(() => _dbContext.Sims.Include(x => x.Network).AsNoTracking().Include(x => x.MobileServer).Include(x =>x.Apps).AsNoTracking().ToListAsync());
            LoadSimCards.Do(sims => Sims = new ObservableCollection<SimCard>(sims)).Subscribe();
            LoadApps = ReactiveCommand.CreateFromTask(() => _dbContext.Apps.Include(x => x.User).ToListAsync());
            LoadApps.Do(apps => Apps = new ObservableCollection<Application>(apps)).Subscribe();

            AddOrUpdateCard = ReactiveCommand.CreateFromTask<SimCard,int>(sim =>
            {
                _dbContext.Entry(sim).State = EntityState.Detached;
                _dbContext.Update(sim);
                return _dbContext.SaveChangesAsync();
            });
            AddOrUpdateCard.Where(rows => rows > 0).Select(_ => Unit.Default).InvokeCommand(LoadSimCards);
            DeleteCard = ReactiveCommand.CreateFromTask<SimCard,int>(sim =>
            {
                _dbContext.Remove(sim);
                return _dbContext.SaveChangesAsync();
            });
            DeleteCard.Where(rows => rows > 0).Select(_ => Unit.Default).InvokeCommand(LoadSimCards);

            AddOrUpdateCard.ThrownExceptions.Select(x => x.Message).ToPropertyEx(this, x => x.Errors);
            DeleteCard.ThrownExceptions.Select(x => x.Message).ToPropertyEx(this, x => x.Errors);
            LoadSimCards.ThrownExceptions.Select(x => x.Message).ToPropertyEx(this, x => x.Errors);

            AddAppSim = ReactiveCommand.CreateFromTask<List<ApplicationSim>, int>(appSims =>
             {
                 _dbContext.AppSims.AddRange(appSims);
                 return _dbContext.SaveChangesAsync();
             });
            DeleteAppSim = ReactiveCommand.CreateFromTask<List<ApplicationSim>, int>(appSims =>
            {
                _dbContext.AppSims.RemoveRange(appSims);
                return _dbContext.SaveChangesAsync();
            });
            AddAppSim.Where(rows => rows > 0).Select(_ => Unit.Default).InvokeCommand(LoadSimCards);
            DeleteAppSim.Where(rows => rows > 0).Select(_ => Unit.Default).InvokeCommand(LoadSimCards);
            AddAppSim.ThrownExceptions.Select(x => x.Message).ToPropertyEx(this, x => x.Errors);
            DeleteAppSim.ThrownExceptions.Select(x => x.Message).ToPropertyEx(this, x => x.Errors);

            LoadServers = ReactiveCommand.CreateFromTask(() => _dbContext.MobileServers.ToListAsync());
            LoadServers.Do(servers => MobileServers = new ObservableCollection<MobileServer>(servers)).Subscribe();
            LoadNetworks.Select(_ => Unit.Default).InvokeCommand(LoadServers);
            LoadServers.Select(_ => Unit.Default).InvokeCommand(LoadApps);
            LoadApps.Select(_ => Unit.Default).InvokeCommand(LoadSimCards);
        }

        [Reactive]
        public bool IsBusy { get; set; }

        public string Errors { [ObservableAsProperty]get; }

        [Reactive]
        public ObservableCollection<SimCard> Sims { get; set; } = new ObservableCollection<SimCard>();

        [Reactive]
        public ObservableCollection<NetworkOperator> Networks { get; set; } = new ObservableCollection<NetworkOperator>();
        [Reactive]
        public ObservableCollection<Application> Apps { get; set; } = new ObservableCollection<Application>();

        [Reactive]
        public ObservableCollection<MobileServer> MobileServers { get; set; } = new ObservableCollection<MobileServer>();

        public ReactiveCommand<Unit, List<SimCard>> LoadSimCards { get; }

        public ReactiveCommand<SimCard, int> AddOrUpdateCard { get; }

        public ReactiveCommand<SimCard, int> DeleteCard { get; }

        public ReactiveCommand<List<ApplicationSim>, int> AddAppSim { get; }

        public ReactiveCommand<List<ApplicationSim>, int> DeleteAppSim { get; }

        public ReactiveCommand<Unit, List<NetworkOperator>> LoadNetworks { get; }

        public ReactiveCommand<Unit, List<Application>> LoadApps { get; }

        public ReactiveCommand<Unit,List<MobileServer>> LoadServers { get; }
    }
}
