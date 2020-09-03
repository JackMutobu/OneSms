using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
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
using System.Linq;
using OneSms.Web.Shared.Enumerations;
using OneSms.Web.Shared.Dtos;
using OneSms.Data;
using OneSms.Hubs;
using OneSms.Services;

namespace OneSms.ViewModels
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
                    await _oneSmsHubContext.Clients.Client(key).SendAsync(SignalRKeys.CancelUssdSession);
                }
                return Unit.Default;
            });

            CheckAirtimeBalance = ReactiveCommand.CreateFromTask<SimCard, Unit>(async sim =>
             {
                 if(!string.IsNullOrEmpty(CurrentServerKey))
                 {
                     var ussdAction = _oneSmsDbContext.UssdActions.Include(x => x.Steps).FirstOrDefault(x => x.ActionType == UssdActionType.AirtimeBalance && x.NetworkId == sim.NetworkId);
                     if(ussdAction != null)
                     {
                         var ussd = new UssdTransactionDto
                         {
                             ActionType = UssdActionType.AirtimeBalance,
                             KeyProblems = ussdAction.KeyProblems.Split(",").ToList(),
                             KeyWelcomes = ussdAction.KeyLogins.Split(",").ToList(),
                             SimId = sim.Id,
                             SimSlot = sim.SimSlot,
                             TimeStamp = DateTime.Now,
                             TransactionState = UssdTransactionState.Sent,
                             UssdNumber = ussdAction.UssdNumber
                         };
                         await _oneSmsHubContext.Clients.Client(CurrentServerKey).SendAsync(SignalRKeys.SendUssd, ussd);
                     }
                     
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

        public ReactiveCommand<SimCard, Unit> CheckAirtimeBalance { get; }
    }
}
