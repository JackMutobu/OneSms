using Microsoft.EntityFrameworkCore;
using OneSms.Online.Data;
using OneSms.Web.Shared.Enumerations;
using OneSms.Web.Shared.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Security.Cryptography.X509Certificates;

namespace OneSms.Online.ViewModels
{
    public class UssdAdminViewModel:ReactiveObject
    {
        private readonly OneSmsDbContext _oneSmsDbContext;

        public UssdAdminViewModel(OneSmsDbContext oneSmsDbContext)
        {
            _oneSmsDbContext = oneSmsDbContext;
            LoadNetworks = ReactiveCommand.CreateFromTask(() => _oneSmsDbContext.Networks.ToListAsync());
            LoadNetworks.Do(networks => 
            Networks = new ObservableCollection<NetworkOperator>(networks)).Subscribe();

            LoadUssdActions = ReactiveCommand.CreateFromTask<(int networkId, int ussdActionType), List<UssdAction>>(async param => await _oneSmsDbContext.UssdActions.Include(x => x.Network)
                    .Where(x => param.networkId == -1 || x.NetworkId == param.networkId
                    && param.ussdActionType == -1 || (int)x.ActionType == param.ussdActionType).ToListAsync());

            LoadUssdActions.Do(ussds => UssdActions = new ObservableCollection<UssdAction>(ussds)).Subscribe();
            LoadUssdActions.ThrownExceptions.Select(x => x.Message).ToPropertyEx(this, x => x.Errors);
            UssdActionTypes = new ObservableCollection<UssdActionType>(Enum.GetValues(typeof(UssdActionType)).Cast<UssdActionType>());

            AddOrUpdateUssd = ReactiveCommand.CreateFromTask<UssdAction, int>(ussd =>
            {
                ussd.CreatedOn = DateTime.UtcNow;
                _oneSmsDbContext.Update(ussd);
                return _oneSmsDbContext.SaveChangesAsync();
            });
            AddOrUpdateUssd.Where(rows => rows > 0).Select(_ => (CurrentNetwork?.Id ?? -1, (int?)CurrentActionType ?? -1)).InvokeCommand(LoadUssdActions);
            AddOrUpdateUssd.ThrownExceptions.Select(x => x.Message).ToPropertyEx(this, x => x.Errors);
            DeleteUssd = ReactiveCommand.CreateFromTask<UssdAction, int>(ussd =>
            {
                _oneSmsDbContext.Remove(ussd);
                return _oneSmsDbContext.SaveChangesAsync();
            });
            DeleteUssd.Where(rows => rows > 0).Select(_ => (CurrentNetwork?.Id ?? -1, (int?)CurrentActionType ?? -1)).InvokeCommand(LoadUssdActions);

           

            LoadUssdSteps = ReactiveCommand.CreateFromTask<int,List<UssdActionStep>>(id => _oneSmsDbContext.UssdActionSteps.Where(x => x.UssdActionId == id).ToListAsync());
            this.WhenAnyValue(x => x.CurrentUssdAction).Where(x => x != null).Select(x => x.Id)
                .InvokeCommand(LoadUssdSteps);
            LoadUssdSteps.Do(ussds => UssdStepActions = new ObservableCollection<UssdActionStep>(ussds)).Subscribe();
            LoadUssdSteps.ThrownExceptions.Select(x => x.Message).ToPropertyEx(this, x => x.Errors);

            AddOrUpdateUssdStep = ReactiveCommand.CreateFromTask<UssdActionStep, int>(ussd =>
            {
                ussd.CreatedOn = DateTime.UtcNow;
                _oneSmsDbContext.Update(ussd);
                return _oneSmsDbContext.SaveChangesAsync();
            });
            AddOrUpdateUssdStep.Where(rows => rows > 0).Select(_ => CurrentUssdAction.Id).InvokeCommand(LoadUssdSteps);
            DeleteUssdStep = ReactiveCommand.CreateFromTask<UssdActionStep, int>(ussd =>
            {
                _oneSmsDbContext.Remove(ussd);
                return _oneSmsDbContext.SaveChangesAsync();
            });
            DeleteUssdStep.Where(rows => rows > 0).Select(_ => CurrentUssdAction.Id).InvokeCommand(LoadUssdSteps);

        }
        [Reactive]
        public ObservableCollection<NetworkOperator> Networks { get; set; } = new ObservableCollection<NetworkOperator>();
        [Reactive]
        public ObservableCollection<UssdAction> UssdActions { get; set; } = new ObservableCollection<UssdAction>();
        [Reactive]
        public ObservableCollection<UssdActionStep> UssdStepActions { get; set; } = new ObservableCollection<UssdActionStep>();
        [Reactive]
        public NetworkOperator CurrentNetwork { get; set; }
        [Reactive]
        public UssdActionType? CurrentActionType { get; set; }
        [Reactive]
        public UssdAction CurrentUssdAction { get; set; }
        [Reactive]
        public bool IsBusy { get; set; }
        [Reactive]
        public ObservableCollection<UssdActionType> UssdActionTypes  { get; set; }

        public string  Errors { [ObservableAsProperty]get; }


        public ReactiveCommand<Unit, List<NetworkOperator>> LoadNetworks { get; }
        public ReactiveCommand<(int networkId,int ussdActionType), List<UssdAction>> LoadUssdActions { get; }
        public ReactiveCommand<UssdAction, int> AddOrUpdateUssd { get; }
        public ReactiveCommand<UssdAction, int> DeleteUssd { get; }

        public ReactiveCommand<int, List<UssdActionStep>> LoadUssdSteps { get; }
        public ReactiveCommand<UssdActionStep, int> AddOrUpdateUssdStep { get; }
        public ReactiveCommand<UssdActionStep, int> DeleteUssdStep { get; }
    }
}
