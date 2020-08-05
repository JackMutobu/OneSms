using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using OneSms.Online.Data;
using OneSms.Online.ViewModels;
using OneSms.Web.Shared.Models;
using System;
using System.Linq;
using System.Reactive.Threading.Tasks;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OneSms.Online.Views.Infrastructure
{
    public partial class AppView
    {
        bool modalVisible = false;
        OneSmsApp app = new OneSmsApp();

        [Inject]
        OneSmsDbContext OneSmsDbContext { get; set; }
        [Inject]
        AuthenticationStateProvider AuthenticationsStateProvider { get; set; }

        protected async override Task OnInitializedAsync()
        {
            base.OnInitialized();
            var authstate = await AuthenticationsStateProvider.GetAuthenticationStateAsync();
            var user = authstate.User;
            ViewModel = new AppViewModel(OneSmsDbContext)
            {
                UserEmail = user.Claims.First(x => x.Type == ClaimTypes.Name).Value
            };
            await ViewModel.LoadApps.Execute(ViewModel.UserEmail).ToTask();
        }

        private void HideModal() => modalVisible = false;

        private async Task Save(EditContext editContext)
        {
            modalVisible = false;
            await AddOrUpdate(app);
            app = new OneSmsApp();
        }
        private void ShowModal(OneSmsApp oneSmsApp)
        {
            modalVisible = true;
            if(app.AppId == Guid.Empty)
            {
                app.Credit = "0";
                app.SmsBalance = "0";
            }
            app.UserEmail = ViewModel.UserEmail;
            app = oneSmsApp ?? new OneSmsApp();
        }
        private async Task AddOrUpdate(OneSmsApp oneSmsApp)
           => await ViewModel.AddOrUpdateApp.Execute(oneSmsApp).ToTask();
        private async Task Delete(OneSmsApp app)
            => await ViewModel.DeleteApp.Execute(app).ToTask();
    }
}
