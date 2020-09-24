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

namespace OneSms.ViewModels.Sms
{
    public class NetworkMessageViewModel: ReactiveObject
    {
        private readonly DataContext _dbContext;

        public NetworkMessageViewModel(DataContext dbContext)
        {
            _dbContext = dbContext;
            Messages = new ObservableCollection<NetworkMessageData>();

            LoadMessages = ReactiveCommand.CreateFromTask(_ => _dbContext.NetworkMessages.ToListAsync());
            LoadMessages.Do(messages => Messages = new ObservableCollection<NetworkMessageData>(messages)).Subscribe();

            DeleteMessage = ReactiveCommand.CreateFromTask<NetworkMessageData, int>(message =>
            {
                _dbContext.Remove(message);
                return _dbContext.SaveChangesAsync();
            });
            DeleteMessage.InvokeCommand(LoadMessages);
        }


        [Reactive]
        public ObservableCollection<NetworkMessageData> Messages { get; set; }

        public ReactiveCommand<NetworkMessageData, int> DeleteMessage { get; }

        public ReactiveCommand<Unit, List<NetworkMessageData>> LoadMessages { get; }

    }
}
