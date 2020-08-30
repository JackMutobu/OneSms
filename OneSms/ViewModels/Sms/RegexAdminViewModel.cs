using Microsoft.EntityFrameworkCore;
using OneSms.Data;
using OneSms.Online.Models;
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

namespace OneSms.ViewModels
{
    public class RegexAdminViewModel:ReactiveObject
    {
        private OneSmsDbContext _oneSmsDbContext;

        public RegexAdminViewModel(OneSmsDbContext oneSmsDbContext)
        {
            _oneSmsDbContext = oneSmsDbContext;
            LoadSmsExtractors = ReactiveCommand.CreateFromTask(() => _oneSmsDbContext.SmsDataExtractors.Include(x => x.Network).ToListAsync());
            LoadSmsExtractors.Do(items => SmsDataExtractors = new ObservableCollection<SmsDataExtractor>(items)).Subscribe();

            LoadNetworks = ReactiveCommand.CreateFromTask(() => _oneSmsDbContext.Networks.ToListAsync());
            LoadNetworks.Do(items => Networks = new ObservableCollection<NetworkOperator>(items)).Subscribe();
            LoadNetworks.Select(_ => Unit.Default).InvokeCommand(LoadSmsExtractors);

            UssdActionTypes = new ObservableCollection<UssdActionType>(Enum.GetValues(typeof(UssdActionType)).Cast<UssdActionType>());

            AddOrUpdateItem = ReactiveCommand.CreateFromTask<SmsDataExtractor, int>(item =>
            {
                item.CreatedOn = DateTime.UtcNow;
                _oneSmsDbContext.Update(item);
                return _oneSmsDbContext.SaveChangesAsync();
            });
            AddOrUpdateItem.Where(rows => rows > 0).Select(_ => Unit.Default).InvokeCommand(LoadSmsExtractors);

            DeleteItem = ReactiveCommand.CreateFromTask<SmsDataExtractor, int>(data =>
            {
                _oneSmsDbContext.SmsDataExtractors.Remove(data);
                return _oneSmsDbContext.SaveChangesAsync();
            });
            DeleteItem.Where(rows => rows > 0).Select(_ => Unit.Default).InvokeCommand(LoadSmsExtractors);
        }

        [Reactive]
        public ObservableCollection<SmsDataExtractor> SmsDataExtractors { get; set; }

        [Reactive]
        public ObservableCollection<NetworkOperator> Networks { get; set; }

        [Reactive]
        public ObservableCollection<UssdActionType> UssdActionTypes { get; set; }

        public ReactiveCommand<Unit,List<SmsDataExtractor>> LoadSmsExtractors { get;}

        public ReactiveCommand<Unit, List<NetworkOperator>> LoadNetworks { get; }

        public ReactiveCommand<SmsDataExtractor,int> AddOrUpdateItem { get; }

        public ReactiveCommand<SmsDataExtractor,int> DeleteItem { get; }
    }
}
