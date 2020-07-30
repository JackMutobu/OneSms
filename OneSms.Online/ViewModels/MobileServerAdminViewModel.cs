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

namespace OneSms.Online.ViewModels
{
    public class MobileServerAdminViewModel:ReactiveObject
    {
        private OneSmsDbContext _oneSmsDbContext;

        public MobileServerAdminViewModel(OneSmsDbContext oneSmsDbContext)
        {
            _oneSmsDbContext = oneSmsDbContext;
            LoadServerMobiles = ReactiveCommand.CreateFromTask(() => _oneSmsDbContext.MobileServers.ToListAsync());
            LoadServerMobiles.Do(serverMobiles => MobileServers = new ObservableCollection<ServerMobile>(serverMobiles)).Subscribe();

            AddOrUpdateServerMobile = ReactiveCommand.CreateFromTask<ServerMobile, int>(server =>
            {
                server.CreatedOn = DateTime.UtcNow;
                _oneSmsDbContext.Update(server);
                return _oneSmsDbContext.SaveChangesAsync();
            });
            AddOrUpdateServerMobile.Where(rows => rows > 0).Select(_ => Unit.Default).InvokeCommand(LoadServerMobiles);

            DeleteServerMobile = ReactiveCommand.CreateFromTask<ServerMobile, int>(server =>
            {
                _oneSmsDbContext.Remove(server);
                return _oneSmsDbContext.SaveChangesAsync();
            });
            DeleteServerMobile.Where(rows => rows > 0).Select(_ => Unit.Default).InvokeCommand(LoadServerMobiles);

            LoadSimCards = ReactiveCommand.CreateFromTask(() => _oneSmsDbContext.Sims.ToListAsync());
            LoadSimCards.Do(sims => SimCards = new ObservableCollection<SimCard>(sims)).Subscribe();
        }

        [Reactive]
        public bool IsBusy { get; set; }

        [Reactive]
        public ObservableCollection<ServerMobile> MobileServers { get; set; } = new ObservableCollection<ServerMobile>();

        [Reactive]
        public ObservableCollection<SimCard> SimCards { get; set; } = new ObservableCollection<SimCard>();

        public ReactiveCommand<Unit, List<ServerMobile>> LoadServerMobiles { get; }

        public ReactiveCommand<Unit, List<SimCard>> LoadSimCards { get; }

        public ReactiveCommand<ServerMobile, int> AddOrUpdateServerMobile { get; }

        public ReactiveCommand<ServerMobile, int> DeleteServerMobile { get; }
    }
}
