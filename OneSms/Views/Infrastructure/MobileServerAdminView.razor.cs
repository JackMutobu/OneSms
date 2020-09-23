using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using OneSms.Data;
using OneSms.Domain;
using OneSms.ViewModels;
using System;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace OneSms.Views.Infrastructure
{
    public partial class MobileServerAdminView
    {
        bool modalVisible = false;
        MobileServer serverMobile = new MobileServer();

        [Inject]
        DataContext DataContext { get; set; } = null!;

        protected async override Task OnInitializedAsync()
        {
            base.OnInitialized();
            ViewModel = new MobileServerAdminViewModel(DataContext);
            await ViewModel.LoadServerMobiles.Execute().ToTask();
            await ViewModel.LoadSimCards.Execute().ToTask();
        }

        private async Task AddOrUpdate(MobileServer serverMobile)
            => await ViewModel.AddOrUpdateServerMobile.Execute(serverMobile).ToTask();

        private async Task Delete(MobileServer serverMobile)
            => await ViewModel.DeleteServerMobile.Execute(serverMobile).ToTask();

        private async Task Save(EditContext editContext)
        {
            modalVisible = false;
            await AddOrUpdate(serverMobile);
            serverMobile = new MobileServer();
        }
        private void ShowModal(MobileServer server)
        {
            modalVisible = true;
            if (server.Id == Guid.Empty)
                server.Secret = Guid.NewGuid().ToString();
            serverMobile = server ?? new MobileServer();
        }
        private void HideModal() => modalVisible = false;

        
    }
}
