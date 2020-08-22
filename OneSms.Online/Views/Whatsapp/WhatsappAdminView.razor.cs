using AntDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.SignalR;
using OneOf;
using OneSms.Online.Data;
using OneSms.Online.Hubs;
using OneSms.Online.Services;
using OneSms.Web.Shared.Dtos;
using OneSms.Web.Shared.Enumerations;
using OneSms.Web.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace OneSms.Online.Views.Whatsapp
{
    public partial class WhatsappAdminView
    {
        WhatsappTransaction transaction = new WhatsappTransaction();

        [Inject]
        OneSmsDbContext OneSmsDbContext { get; set; }

        [Inject]
        IHubContext<OneSmsHub> OneSmsHubContext { get; set; }

        [Inject]
        ServerConnectionService ServerConnectionService { get; set; }

        [Inject]
        HubEventService HubEventService { get; set; }

        protected async override Task OnInitializedAsync()
        {
            ViewModel = new ViewModels.Whatsapp.WhatsappAdminViewModel(OneSmsDbContext, ServerConnectionService, OneSmsHubContext, HubEventService);
            await ViewModel.LoadMobileServers.Execute().ToTask();
        }

        private async Task OnFinish(EditContext editContext)
        {
            transaction.TransactionState = MessageTransactionState.Sending;
            transaction.MobileServerId = ViewModel.MobileServer.Id;
            await ViewModel.AddTransaction.Execute(transaction).ToTask();
            transaction = new WhatsappTransaction() { RecieverNumber = transaction.RecieverNumber, Title = transaction.Title, Body = transaction.Body };
        }

        private void OnServerSelectChange(OneOf<string, IEnumerable<string>, LabeledValue, IEnumerable<LabeledValue>> value, OneOf<SelectOption, IEnumerable<SelectOption>> option)
        {
            var server = ViewModel.MobileServers.First(x => x.Id == int.Parse(value.Value.ToString()));
            ViewModel.MobileServer = server;
        }
    }
}
