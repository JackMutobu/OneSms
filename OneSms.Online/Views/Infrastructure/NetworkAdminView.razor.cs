using AntDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using OneSms.Online.Data;
using OneSms.Online.ViewModels;
using OneSms.Web.Shared.Models;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace OneSms.Online.Views.Infrastructure
{
    public partial class NetworkAdminView
    {
        bool modalVisible = false;
        NetworkOperator network = new NetworkOperator();

        [Inject]
        OneSmsDbContext OneSmsDbContext { get; set; }

        protected async override Task OnInitializedAsync()
        {
            base.OnInitialized();
            ViewModel = new NetworkAdminViewModel(OneSmsDbContext);
            await ViewModel.LoadNetworks.Execute().ToTask();
        }

        private async Task AddOrUpdate(NetworkOperator network) 
            => await ViewModel.AddOrUpdateNetwork.Execute(network).ToTask();

        private async Task Delete(NetworkOperator network)
            => await ViewModel.DeleteNetwork.Execute(network).ToTask();

        private async Task Save(EditContext editContext)
        {
            modalVisible = false;
            await AddOrUpdate(network);
            network = new NetworkOperator();
        }
        private void ShowModal(NetworkOperator networkOperator)
        {
            modalVisible = true;
            network = networkOperator ?? new NetworkOperator();
        }
        private void HideModal() => modalVisible = false;
    }
}
