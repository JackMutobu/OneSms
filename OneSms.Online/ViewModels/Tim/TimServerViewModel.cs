using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using OneSms.Online.Data;
using OneSms.Online.Hubs;
using OneSms.Online.Services;
using OneSms.Web.Shared.Constants;
using OneSms.Web.Shared.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Linq;

namespace OneSms.Online.ViewModels
{
    public class TimServerViewModel:ReactiveObject
    {
        private OneSmsDbContext _oneSmsDbContext;
        private IHubContext<OneSmsHub> _oneSmsHubContext;
        private HubEventService _hubEventService;
        private ServerConnectionService _serverConnectionService;
        public TimServerViewModel(OneSmsDbContext oneSmsDbContext, IHubContext<OneSmsHub> oneSmsHubContext, ServerConnectionService serverConnectionService)
        {
            _oneSmsDbContext = oneSmsDbContext;
            _oneSmsHubContext = oneSmsHubContext;
            _serverConnectionService = serverConnectionService;

            LoadMobileServers = ReactiveCommand.CreateFromTask<Unit, List<ServerMobile>>(servers => _oneSmsDbContext.MobileServers.Include(x => x.Sims).Where(x => x.IsTimServer).ToListAsync());
            LoadMobileServers.Do(servers => MobileServers = new ObservableCollection<ServerMobile>(servers)).Subscribe();
            LoadMobileServers.ThrownExceptions.Select(x => x.Message).ToPropertyEx(this, x => x.Errors);

            AddOrUpdate = ReactiveCommand.CreateFromTask<ServerMobile, Unit>(async server =>
             {
                 server.CreatedOn = DateTime.UtcNow;
                 server.IsTimServer = true;
                 await _oneSmsDbContext.AddAsync(server);
                 await _oneSmsDbContext.SaveChangesAsync();
                 return Unit.Default;
             });
            AddOrUpdate.InvokeCommand(LoadMobileServers);
            AddOrUpdate.ThrownExceptions.Select(x => x.Message).ToPropertyEx(this, x => x.Errors);

            Delete = ReactiveCommand.CreateFromTask<ServerMobile, int>(item =>
            {
                _oneSmsDbContext.Remove(item);
                return _oneSmsDbContext.SaveChangesAsync();
            });
            Delete.Where(rows => rows > 0).Select(_ => Unit.Default).InvokeCommand(LoadMobileServers);
            Delete.ThrownExceptions.Select(x => x.Message).ToPropertyEx(this, x => x.Errors);

            GetOnlineServer = ReactiveCommand.Create<List<ServerMobile>, string>(servers => 
            {
                var serverConnectionId = string.Empty;
                servers.ForEach(server => _serverConnectionService.ConnectedServers.TryGetValue(server.Key.ToString(), out serverConnectionId));
                return serverConnectionId;
            });
            GetOnlineServer.Do(serverKey => CurrentServerKey = serverKey).Subscribe();
            LoadMobileServers.InvokeCommand(GetOnlineServer);

            CancelUssdSession = ReactiveCommand.CreateFromTask<string, Unit>(async key =>
            {
                if (!string.IsNullOrEmpty(key))
                {
                    var serverConnectionId = string.Empty;
                    if (_serverConnectionService.ConnectedServers.TryGetValue(key, out serverConnectionId))
                        await _oneSmsHubContext.Clients.Client(serverConnectionId).SendAsync(SignalRKeys.CancelUssdSession);
                }
                return Unit.Default;
            });
        }

        [Reactive]
        public string CurrentServerKey { get; set; }

        [Reactive]
        public ObservableCollection<ServerMobile> MobileServers { get; set; } = new ObservableCollection<ServerMobile>();

        public ReactiveCommand<Unit, List<ServerMobile>> LoadMobileServers { get; }

        public ReactiveCommand<ServerMobile, Unit> AddOrUpdate{ get; }

        public ReactiveCommand<List<ServerMobile>, string> GetOnlineServer { get; }

        public string Errors { [ObservableAsProperty]get; }

        [Reactive]
        public bool IsBusy { get; set; }

        public ReactiveCommand<ServerMobile, int> Delete { get; }

        public ReactiveCommand<string, Unit> CancelUssdSession { get; }
    }
}
