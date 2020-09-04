using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.SignalR;
using OneSms.Data;
using OneSms.Hubs;
using OneSms.Services;
using OneSms.ViewModels;
using OneSms.Web.Shared.Models;
using System;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace OneSms.Views.Tim
{
    public partial class TimTransactionView
    {
        bool modalVisible = false;
        TimTransaction transaction = new TimTransaction();

        [Inject]
        OneSmsDbContext OneSmsDbContext { get; set; } = null!;
        [Inject]
        IHubContext<OneSmsHub> OneSmsHubContext  { get; set; } = null!;
        [Inject]
        HubEventService HubEventService { get; set; } = null!;
        [Inject]
        ServerConnectionService ServerConnectionService { get; set; } = null!;

        private void HideModal() => modalVisible = false;

        protected async override Task OnInitializedAsync()
        {
            ViewModel = new TimTransactionViewModel(OneSmsDbContext, OneSmsHubContext,HubEventService,ServerConnectionService);
            await ViewModel.LoadTransactions.Execute().ToTask();
        }

        private void ShowModal(TimTransaction timTransaction)
        {
            modalVisible = true;
            transaction.CreatedOn = DateTime.UtcNow;
            transaction = timTransaction ?? new TimTransaction();
        }

        private async Task Save(EditContext editContext)
        {
            modalVisible = false;
            await AddOrUpdate(transaction);
            transaction = new TimTransaction();
        }

        private async Task Delete(TimTransaction transaction)
           => await ViewModel.Delete.Execute(transaction).ToTask();

        private async Task AddOrUpdate(TimTransaction transaction)
           => await ViewModel.AddOrUpdate.Execute(transaction).ToTask();

        private async Task Refresh() => await ViewModel.LoadTransactions.ToTask();
    }
}
