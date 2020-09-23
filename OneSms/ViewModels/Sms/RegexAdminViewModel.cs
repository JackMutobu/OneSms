using OneSms.Contracts.V1.Enumerations;
using OneSms.Domain;
using OneSms.Services;
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
    public class RegexAdminViewModel:ReactiveObject
    {
        private readonly IMessageExtractionService _messageExtractionService;
        private readonly INetworkService _networkService;

        public RegexAdminViewModel(IMessageExtractionService messageExtractionService, INetworkService networkService)
        {
            _messageExtractionService = messageExtractionService;
            _networkService = networkService;
            Networks = new ObservableCollection<NetworkOperator>();
            MessageExtractors = new ObservableCollection<NetworkMessageExtractor>();
            LoadExtractors = ReactiveCommand.CreateFromTask(() => _messageExtractionService.GetExtractors());
            NetworkActionTypes = new ObservableCollection<NetworkActionType>(Enum.GetValues(typeof(NetworkActionType)).Cast<NetworkActionType>());

            LoadExtractors.Do(items => MessageExtractors = new ObservableCollection<NetworkMessageExtractor>(items)).Subscribe();
            LoadExtractors.ThrownExceptions.Select(x => x.Message).ToPropertyEx(this, x => x.Errors);

            LoadNetworks = ReactiveCommand.CreateFromTask(() => _networkService.GetNetworks());
            LoadNetworks.Do(items => Networks = new ObservableCollection<NetworkOperator>(items)).Subscribe();
            LoadNetworks.Select(_ => Unit.Default).InvokeCommand(LoadExtractors);
            LoadNetworks.ThrownExceptions.Select(x => x.Message).ToPropertyEx(this, x => x.Errors);



            AddOrUpdateItem = ReactiveCommand.CreateFromTask<NetworkMessageExtractor, NetworkMessageExtractor>(_messageExtractionService.AddExtractor);
            AddOrUpdateItem.Do(x =>
            {
                MessageExtractors.Add(x);
                MessageExtractors = new ObservableCollection<NetworkMessageExtractor>(MessageExtractors.OrderByDescending(x => x.Id));
            }).Subscribe();
            AddOrUpdateItem.ThrownExceptions.Select(x => x.Message).ToPropertyEx(this, x => x.Errors);

            DeleteItem = ReactiveCommand.CreateFromTask<NetworkMessageExtractor, int>(_messageExtractionService.DeleteExtractor);
            DeleteItem.Where(rows => rows > 0).Select(_ => Unit.Default).InvokeCommand(LoadExtractors);
            DeleteItem.ThrownExceptions.Select(x => x.Message).ToPropertyEx(this, x => x.Errors);
        }

        public string? Errors { [ObservableAsProperty]get; }

        [Reactive]
        public ObservableCollection<NetworkMessageExtractor> MessageExtractors { get; set; }

        [Reactive]
        public ObservableCollection<NetworkOperator> Networks { get; set; }

        [Reactive]
        public ObservableCollection<NetworkActionType> NetworkActionTypes { get; set; }

        public ReactiveCommand<Unit,List<NetworkMessageExtractor>> LoadExtractors { get;}

        public ReactiveCommand<Unit, List<NetworkOperator>> LoadNetworks { get; }

        public ReactiveCommand<NetworkMessageExtractor, NetworkMessageExtractor> AddOrUpdateItem { get; }

        public ReactiveCommand<NetworkMessageExtractor, int> DeleteItem { get; }
    }
}
