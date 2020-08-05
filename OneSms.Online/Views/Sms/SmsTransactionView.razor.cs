using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using OneSms.Online.Data;
using OneSms.Online.Hubs;
using OneSms.Online.Services;
using OneSms.Web.Shared.Models;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace OneSms.Online.Views.Sms
{
    public partial class SmsTransactionView
    {
        [Inject]
        OneSmsDbContext OneSmsDbContext { get; set; }

        [Inject]
        IHubContext<OneSmsHub> OneSmsHubContext { get; set; }

        [Inject]
        HubEventService SmsHubEventService { get; set; }

        protected async override Task OnInitializedAsync()
        {
            ViewModel = new ViewModels.SmsTransactionViewModel(OneSmsDbContext, OneSmsHubContext,SmsHubEventService);
            await ViewModel.LoadSmsTransactions.Execute().ToTask();
        }
        private async Task Delete(SmsTransaction smsTransaction)
            => await ViewModel.DeleteTransaction.Execute(smsTransaction).ToTask();
    }
}
