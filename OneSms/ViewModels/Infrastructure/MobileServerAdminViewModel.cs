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
using SimCard = OneSms.Domain.SimCard;

namespace OneSms.ViewModels
{
    public class MobileServerAdminViewModel:ReactiveObject
    {
        private DataContext _dbContext;

        public MobileServerAdminViewModel(DataContext dbContext)
        {
            _dbContext = dbContext;
            LoadServerMobiles = ReactiveCommand.CreateFromTask(() => _dbContext.MobileServers.ToListAsync());
            LoadServerMobiles.Do(serverMobiles => MobileServers = new ObservableCollection<MobileServer>(serverMobiles)).Subscribe();

            AddOrUpdateServerMobile = ReactiveCommand.CreateFromTask<MobileServer, int>(server =>
            {
                _dbContext.Update(server);
                return _dbContext.SaveChangesAsync();
            });
            AddOrUpdateServerMobile.Where(rows => rows > 0).Select(_ => Unit.Default).InvokeCommand(LoadServerMobiles);

            DeleteServerMobile = ReactiveCommand.CreateFromTask<MobileServer, int>(server =>
            {
                _dbContext.Remove(server);
                return _dbContext.SaveChangesAsync();
            });
            DeleteServerMobile.Where(rows => rows > 0).Select(_ => Unit.Default).InvokeCommand(LoadServerMobiles);

            LoadSimCards = ReactiveCommand.CreateFromTask(() => _dbContext.Sims.ToListAsync());
            LoadSimCards.Do(sims => SimCards = new ObservableCollection<SimCard>(sims)).Subscribe();
        }

        public string Errors { [ObservableAsProperty]get; }

        [Reactive]
        public bool IsBusy { get; set; }

        [Reactive]
        public ObservableCollection<MobileServer> MobileServers { get; set; } = new ObservableCollection<MobileServer>();

        [Reactive]
        public ObservableCollection<SimCard> SimCards { get; set; } = new ObservableCollection<SimCard>();

        public ReactiveCommand<Unit, List<MobileServer>> LoadServerMobiles { get; }

        public ReactiveCommand<Unit, List<SimCard>> LoadSimCards { get; }

        public ReactiveCommand<MobileServer, int> AddOrUpdateServerMobile { get; }

        public ReactiveCommand<MobileServer, int> DeleteServerMobile { get; }
    }
}
