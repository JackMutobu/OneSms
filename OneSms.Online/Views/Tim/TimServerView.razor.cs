using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.SignalR;
using OneSms.Online.Data;
using OneSms.Online.Hubs;
using OneSms.Online.Services;
using OneSms.Online.ViewModels;
using OneSms.Web.Shared.Models;
using System;
using System.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace OneSms.Online.Views.Tim
{
    public partial class TimServerView
    {
        bool modalVisible = false;
        ServerMobile serverMobile = new ServerMobile();

        [Inject]
        OneSmsDbContext OneSmsDbContext { get; set; }

        [Inject]
        IHubContext<OneSmsHub> OneSmsHubContext { get; set; }

        [Inject]
        ServerConnectionService ServerConnectionService { get; set; }

        protected async override Task OnInitializedAsync()
        {
            base.OnInitialized();
            ViewModel = new TimServerViewModel(OneSmsDbContext, OneSmsHubContext, ServerConnectionService);
            await ViewModel.LoadMobileServers.Execute().ToTask();
        }

        private async Task AddOrUpdate(ServerMobile serverMobile)
            => await ViewModel.AddOrUpdate.Execute(serverMobile).ToTask();

        private async Task Delete(ServerMobile serverMobile)
            => await ViewModel.Delete.Execute(serverMobile).ToTask();

        private async Task Save(EditContext editContext)
        {
            modalVisible = false;
            await AddOrUpdate(serverMobile);
            serverMobile = new ServerMobile();
        }

        private void ShowModal(ServerMobile server)
        {
            modalVisible = true;
            if (server.Key == Guid.Empty)
            {
                serverMobile.Key = Guid.NewGuid();
                server.IsTimServer = true;
                server.UserEmail = "tim@onesms.com";
            }

            serverMobile = server ?? new ServerMobile();
        }

        private void HideModal() => modalVisible = false;

        private async Task CancelUssdOperation(string serverKey)
        {
            await ViewModel.CancelUssdSession.Execute(serverKey).ToTask();
        }

        private async Task CheckAirtimeBalance(SimCard sim)
        {
            await ViewModel.CheckAirtimeBalance.Execute(sim).ToTask();
        }

        private async Task Refresh()
        {
            await ViewModel.GetOnlineServer.Execute(ViewModel.MobileServers.ToList()).ToTask();
        }
    }
}
