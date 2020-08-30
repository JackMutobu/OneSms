using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using OneSms.Data;
using OneSms.Domain;
using OneSms.ViewModels;
using OneSms.Web.Shared.Models;
using System;
using System.Linq;
using System.Reactive.Threading.Tasks;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OneSms.Views.Infrastructure
{
    public partial class AppView
    {
        bool modalVisible = false;
        Application app = new Application();

        [Inject]
        DataContext DbContext { get; set; }
        [Inject]
        AuthenticationStateProvider AuthenticationsStateProvider { get; set; }

        protected async override Task OnInitializedAsync()
        {
            base.OnInitialized();
            var authstate = await AuthenticationsStateProvider.GetAuthenticationStateAsync();
            var user = authstate.User;
            ViewModel = new AppViewModel(DbContext)
            {
                UserId = user.Claims.First(x => x.Type == "id").Value
            };
            await ViewModel.LoadApps.Execute(ViewModel.UserId).ToTask();
        }

        private void HideModal() => modalVisible = false;

        private async Task Save(EditContext editContext)
        {
            modalVisible = false;
            await AddOrUpdate(app);
            app = new Application();
        }
        private void ShowModal(Application application)
        {
            modalVisible = true;
            if (app.Id == Guid.Empty)
            {
                app.Credit = 0;
                app.Secret = Guid.NewGuid();
                app.UserId = ViewModel.UserId;
                app.CreatedOn = DateTime.UtcNow;
            }
            app = application ?? new Application();
        }
        private async Task AddOrUpdate(Application app)
           => await ViewModel.AddOrUpdateApp.Execute(app).ToTask();
        private async Task Delete(Application app)
            => await ViewModel.DeleteApp.Execute(app).ToTask();
    }
}
