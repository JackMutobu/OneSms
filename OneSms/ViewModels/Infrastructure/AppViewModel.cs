using Microsoft.EntityFrameworkCore;
using OneSms.Data;
using OneSms.Domain;
using OneSms.Web.Shared.Constants;
using OneSms.Web.Shared.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;

namespace OneSms.ViewModels
{
    public class AppViewModel:ReactiveObject
    {
        private DataContext _dbContext;

        public AppViewModel(DataContext context)
        {
            _dbContext = context;
            LoadApps = ReactiveCommand.CreateFromTask<string,List<Application>>(userId =>
            {
                var user = _dbContext.Users.FirstOrDefault(x => x.Id == userId);
                if(user != null)
                {
                    if (user.Role == UserRoles.SuperAdmin)
                        return _dbContext.Apps.Include(x => x.User).ToListAsync();
                }
                return _dbContext.Apps.Include(x => x.User).Where(x => x.UserId == userId).ToListAsync();
            });
            LoadApps.Do(apps => Apps = new ObservableCollection<Application>(apps)).Subscribe();
            LoadApps.ThrownExceptions.Select(x => x.Message).ToPropertyEx(this, x => x.Errors);
            AddOrUpdateApp = ReactiveCommand.CreateFromTask<Application, int>(app =>
            {
                _dbContext.Update(app);

                return _dbContext.SaveChangesAsync();
            });

            AddOrUpdateApp.Where(rows => rows > 0).Select(_ => UserId).InvokeCommand(LoadApps);
            AddOrUpdateApp.ThrownExceptions.Select(x => x.Message).ToPropertyEx(this, x => x.Errors);

            DeleteApp = ReactiveCommand.CreateFromTask<Application, int>(app =>
            {
                _dbContext.Remove(app);
                return _dbContext.SaveChangesAsync();
            });
            DeleteApp.Where(rows => rows > 0).Select(_ => UserId).InvokeCommand(LoadApps);
            DeleteApp.ThrownExceptions.Select(x => x.Message).ToPropertyEx(this, x => x.Errors);
        }

        [Reactive]
        public ObservableCollection<Application> Apps { get; set; } = new ObservableCollection<Application>();
        public string? Errors { [ObservableAsProperty]get; }

        public string? UserId { get; set; }

        public ReactiveCommand<string, List<Application>> LoadApps { get; }

        public ReactiveCommand<Application, int> AddOrUpdateApp { get; }

        public ReactiveCommand<Application, int> DeleteApp { get; }
    }
}
