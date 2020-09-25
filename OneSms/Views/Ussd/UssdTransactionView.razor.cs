using Microsoft.AspNetCore.Components;
using OneSms.Data;
using OneSms.Domain;
using OneSms.Services;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace OneSms.Views.Ussd
{
    public partial class UssdTransactionView
    {
        [Inject]
        DataContext DataContext { get; set; } = null!;

        [Inject]
        HubEventService HubEventService { get; set; } = null!;

        protected async override Task OnInitializedAsync()
        {
            ViewModel = new ViewModels.UssdTransactionViewModel(DataContext, HubEventService);
            await ViewModel.LoadTransactions.Execute().ToTask();
        }
        private async Task Delete(UssdTransaction transaction)
            => await ViewModel.DeleteTransaction.Execute(transaction).ToTask();
    }
}
