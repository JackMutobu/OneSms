using Microsoft.EntityFrameworkCore;
using OneSms.Online.Data;
using OneSms.Web.Shared.Constants;
using OneSms.Web.Shared.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;

namespace OneSms.Online.ViewModels
{
    public class AppViewModel:ReactiveObject
    {
        private OneSmsDbContext _oneSmsDbContext;

        public AppViewModel(OneSmsDbContext oneSmsDbContext)
        {
            _oneSmsDbContext = oneSmsDbContext;
            LoadApps = ReactiveCommand.CreateFromTask<string,List<OneSmsApp>>(email =>
            {
                var user = _oneSmsDbContext.Users.First(x => x.Email == email);
                if (user.Role == UserRoles.SuperAdmin)
                    return _oneSmsDbContext.Apps.ToListAsync();
                return _oneSmsDbContext.Apps.Where(x => x.UserEmail == user.Email).ToListAsync();
            });
            LoadApps.Do(apps => Apps = new ObservableCollection<OneSmsApp>(apps)).Subscribe();
            LoadApps.ThrownExceptions.Select(x => x.Message).ToPropertyEx(this, x => x.Errors);
            AddOrUpdateApp = ReactiveCommand.CreateFromTask<OneSmsApp, int>(app =>
            {
                app.CreatedOn = DateTime.UtcNow;
                _oneSmsDbContext.Update(app);

                return _oneSmsDbContext.SaveChangesAsync();
            });
            AddOrUpdateApp.Where(rows => rows > 0).Select(_ => UserEmail).InvokeCommand(LoadApps);
            AddOrUpdateApp.ThrownExceptions.Select(x => x.Message).ToPropertyEx(this, x => x.Errors);
            DeleteApp = ReactiveCommand.CreateFromTask<OneSmsApp, int>(app =>
            {
                _oneSmsDbContext.Remove(app);
                return _oneSmsDbContext.SaveChangesAsync();
            });
            DeleteApp.Where(rows => rows > 0).Select(_ => UserEmail).InvokeCommand(LoadApps);
            DeleteApp.ThrownExceptions.Select(x => x.Message).ToPropertyEx(this, x => x.Errors);
        }

        [Reactive]
        public ObservableCollection<OneSmsApp> Apps { get; set; } = new ObservableCollection<OneSmsApp>();
        public string Errors { [ObservableAsProperty]get; }

        public string UserEmail { get; set; }

        public ReactiveCommand<string, List<OneSmsApp>> LoadApps { get; }

        public ReactiveCommand<OneSmsApp, int> AddOrUpdateApp { get; }

        public ReactiveCommand<OneSmsApp, int> DeleteApp { get; }
    }
}
