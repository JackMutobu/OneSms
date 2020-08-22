using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using OneSms.Online.Data;
using OneSms.Online.Hubs;
using OneSms.Online.Services;
using OneSms.Online.ViewModels.Whatsapp;
using OneSms.Web.Shared.Models;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace OneSms.Online.Views.Whatsapp
{
    public partial class WhatsappTransactionView
    {
        [Inject]
        OneSmsDbContext OneSmsDbContext { get; set; }

        protected async override Task OnInitializedAsync()
        {
            ViewModel = new WhastappTransactionViewModel(OneSmsDbContext);
            await ViewModel.LoadTransactions.Execute().ToTask();
        }
        private async Task Delete(WhatsappTransaction transaction)
            => await ViewModel.DeleteTransaction.Execute(transaction).ToTask();
    }
}
