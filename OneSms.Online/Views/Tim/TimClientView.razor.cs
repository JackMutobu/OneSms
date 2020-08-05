using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using OneSms.Online.Data;
using OneSms.Web.Shared.Models;
using System;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using OneSms.Online.ViewModels;

namespace OneSms.Online.Views.Tim
{
    public partial class TimClientView
    {
        bool modalVisible = false;
        TimClient client = new TimClient();

        [Inject]
        OneSmsDbContext OneSmsDbContext { get; set; }

        protected async override Task OnInitializedAsync()
        {
            base.OnInitialized();
            ViewModel = new TimClientViewModel(OneSmsDbContext);
            await ViewModel.LoadClients.Execute().ToTask();
        }

        private async Task AddOrUpdate(TimClient client)
            => await ViewModel.AddOrUpdate.Execute(client).ToTask();

        private async Task Delete(TimClient client)
            => await ViewModel.Delete.Execute(client).ToTask();

        private async Task Save(EditContext editContext)
        {
            modalVisible = false;
            await AddOrUpdate(client);
            client = new TimClient();
        }

        private void ShowModal(TimClient timClient)
        {
            modalVisible = true;
            client.CreatedOn = DateTime.UtcNow;
            client = timClient ?? new TimClient();
        }

        private void HideModal() => modalVisible = false;

        private void OnChange(DateTime value, string dateString)
        {
            client.ActivationTime = value;
        }
    }
}
