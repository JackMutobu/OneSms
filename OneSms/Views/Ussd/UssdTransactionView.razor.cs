using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using OneSms.Data;
using OneSms.Hubs;
using OneSms.Online.Services;
using OneSms.Web.Shared.Models;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace OneSms.Views.Ussd
{
    public partial class UssdTransactionView
    {
        [Inject]
        OneSmsDbContext OneSmsDbContext { get; set; }

        [Inject]
        IHubContext<OneSmsHub> OneSmsHubContext { get; set; }

        [Inject]
        HubEventService SmsHubEventService { get; set; }

        protected async override Task OnInitializedAsync()
        {
            ViewModel = new ViewModels.UssdTransactionViewModel(OneSmsDbContext, OneSmsHubContext, SmsHubEventService);
            await ViewModel.LoadTransactions.Execute().ToTask();
        }
        private async Task Delete(UssdTransaction transaction)
            => await ViewModel.DeleteTransaction.Execute(transaction).ToTask();
    }
}
