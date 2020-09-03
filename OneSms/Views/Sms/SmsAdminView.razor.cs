using AntDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.SignalR;
using OneOf;
using OneSms.Data;
using OneSms.Hubs;
using OneSms.Online.Services;
using OneSms.Services;
using OneSms.Web.Shared.Enumerations;
using OneSms.Web.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace OneSms.Views.Sms
{
    public partial class SmsAdminView
    {
        SmsTransaction transaction = new SmsTransaction();

        [Inject]
        OneSmsDbContext OneSmsDbContext { get; set; }

        [Inject]
        IHubContext<OneSmsHub> OneSmsHubContext { get; set; }

        [Inject]
        ServerConnectionService ServerConnectionService { get; set; }

        [Inject]
        HubEventService SmsHubEventService { get; set; }

        protected async override Task OnInitializedAsync()
        {
            ViewModel = new ViewModels.SmsAdminViewModel(OneSmsDbContext, ServerConnectionService, OneSmsHubContext,SmsHubEventService);
            await ViewModel.LoadSimCards.Execute().ToTask();
        }

        private async Task OnFinish(EditContext editContext)
        {
            transaction.TransactionState = MessageTransactionState.Sending;
            transaction.StartTime = DateTime.UtcNow;
            transaction.OneSmsAppId = ViewModel.SelectedSimCard.Apps.First().AppId;
            transaction.MobileServerId = ViewModel.SelectedSimCard.MobileServerId;
            await ViewModel.AddSmsTransaction.Execute(transaction).ToTask();
            transaction = new SmsTransaction { Body = transaction.Body,SenderNumber =  transaction.SenderNumber, RecieverNumber = transaction.RecieverNumber, Title = transaction.Title };
        }

        private void OnSimSelectChange(OneOf<string, IEnumerable<string>, LabeledValue, IEnumerable<LabeledValue>> value, OneOf<SelectOption, IEnumerable<SelectOption>> option)
        {
            var sim = ViewModel.Sims.First(x => x.Id == int.Parse(value.Value.ToString()));
            ViewModel.SelectedSimCard = sim;
            transaction.SenderNumber = sim.Number;
        }
    }
}
