using AntDesign;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OneSms.Online.Data;
using OneSms.Web.Shared.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace OneSms.Online.ViewModels
{
    public class SimAdminViewModel:ReactiveObject
    {
        private OneSmsDbContext _oneSmsDbContext;

        public SimAdminViewModel(OneSmsDbContext oneSmsDbContext)
        {
            _oneSmsDbContext = oneSmsDbContext;
            Sims = new ObservableCollection<SimCard>();
            Networks = new ObservableCollection<NetworkOperator>();
            Apps = new ObservableCollection<OneSmsApp>();

            LoadNetworks = ReactiveCommand.CreateFromTask(() => _oneSmsDbContext.Networks.ToListAsync());
            LoadNetworks.Do(networks => Networks = new ObservableCollection<NetworkOperator>(networks)).Subscribe();
            LoadSimCards = ReactiveCommand.CreateFromTask(() => _oneSmsDbContext.Sims.AsNoTracking().Include(x => x.MobileServer).Include(x =>x.Apps).AsNoTracking().ToListAsync());
            LoadSimCards.Do(sims => Sims = new ObservableCollection<SimCard>(sims)).Subscribe();
            LoadApps = ReactiveCommand.CreateFromTask(() => _oneSmsDbContext.Apps.ToListAsync());
            LoadApps.Do(apps => Apps = new ObservableCollection<OneSmsApp>(apps)).Subscribe();

            AddOrUpdateCard = ReactiveCommand.CreateFromTask<SimCard,int>(sim =>
            {
                sim.CreatedOn = DateTime.UtcNow;
                _oneSmsDbContext.Entry(sim).State = EntityState.Detached;
                _oneSmsDbContext.Update(sim);
                return _oneSmsDbContext.SaveChangesAsync();
            });
            AddOrUpdateCard.Where(rows => rows > 0).Select(_ => Unit.Default).InvokeCommand(LoadSimCards);
            DeleteCard = ReactiveCommand.CreateFromTask<SimCard,int>(sim =>
            {
                _oneSmsDbContext.Remove(sim);
                return _oneSmsDbContext.SaveChangesAsync();
            });
            DeleteCard.Where(rows => rows > 0).Select(_ => Unit.Default).InvokeCommand(LoadSimCards);

            AddOrUpdateCard.ThrownExceptions.Select(x => x.Message).ToPropertyEx(this, x => x.Errors);
            DeleteCard.ThrownExceptions.Select(x => x.Message).ToPropertyEx(this, x => x.Errors);
            LoadSimCards.ThrownExceptions.Select(x => x.Message).ToPropertyEx(this, x => x.Errors);

            AddAppSim = ReactiveCommand.CreateFromTask<List<AppSim>, int>(appSims =>
             {
                 _oneSmsDbContext.AppSims.AddRange(appSims);
                 return _oneSmsDbContext.SaveChangesAsync();
             });
            DeleteAppSim = ReactiveCommand.CreateFromTask<List<AppSim>, int>(appSims =>
            {
                _oneSmsDbContext.AppSims.RemoveRange(appSims);
                return _oneSmsDbContext.SaveChangesAsync();
            });
            AddAppSim.Where(rows => rows > 0).Select(_ => Unit.Default).InvokeCommand(LoadSimCards);
            DeleteAppSim.Where(rows => rows > 0).Select(_ => Unit.Default).InvokeCommand(LoadSimCards);
            AddAppSim.ThrownExceptions.Select(x => x.Message).ToPropertyEx(this, x => x.Errors);
            DeleteAppSim.ThrownExceptions.Select(x => x.Message).ToPropertyEx(this, x => x.Errors);

            LoadServers = ReactiveCommand.CreateFromTask(() => _oneSmsDbContext.MobileServers.ToListAsync());
            LoadServers.Do(servers => MobileServers = new ObservableCollection<ServerMobile>(servers)).Subscribe();
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
        public ObservableCollection<OneSmsApp> Apps { get; set; } = new ObservableCollection<OneSmsApp>();

        [Reactive]
        public ObservableCollection<ServerMobile> MobileServers { get; set; } = new ObservableCollection<ServerMobile>();

        public ReactiveCommand<Unit, List<SimCard>> LoadSimCards { get; }

        public ReactiveCommand<SimCard, int> AddOrUpdateCard { get; }

        public ReactiveCommand<SimCard, int> DeleteCard { get; }

        public ReactiveCommand<List<AppSim>, int> AddAppSim { get; }

        public ReactiveCommand<List<AppSim>, int> DeleteAppSim { get; }

        public ReactiveCommand<Unit, List<NetworkOperator>> LoadNetworks { get; }

        public ReactiveCommand<Unit, List<OneSmsApp>> LoadApps { get; }

        public ReactiveCommand<Unit,List<ServerMobile>> LoadServers { get; }
    }
}
