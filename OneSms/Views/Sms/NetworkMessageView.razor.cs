using Microsoft.AspNetCore.Components;
using OneSms.Data;
using OneSms.Domain;
using OneSms.ViewModels.Sms;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace OneSms.Views.Sms
{
    public partial class NetworkMessageView
    {
        [Inject]
        DataContext DataContext { get; set; } = null!;

        protected async override Task OnInitializedAsync()
        {
            ViewModel = new NetworkMessageViewModel(DataContext);
            await ViewModel.LoadMessages.Execute().ToTask();
        }
        private async Task Delete(NetworkMessageData message)
            => await ViewModel.DeleteMessage.Execute(message).ToTask();

    }
}
