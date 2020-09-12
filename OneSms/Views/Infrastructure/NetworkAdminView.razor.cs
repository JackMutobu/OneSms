using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using OneSms.Data;
using OneSms.Domain;
using OneSms.ViewModels;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace OneSms.Views.Infrastructure
{
    public partial class NetworkAdminView
    {
        bool modalVisible;
        NetworkOperator network = new NetworkOperator();

        [Inject]
        DataContext DataContext { get; set; } = null!;

        protected async override Task OnInitializedAsync()
        {
            base.OnInitialized();
            ViewModel = new NetworkAdminViewModel(DataContext);
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
