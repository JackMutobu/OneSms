using AntDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.SignalR;
using OneOf;
using OneSms.Contracts.V1.Enumerations;
using OneSms.Data;
using OneSms.Domain;
using OneSms.Hubs;
using OneSms.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace OneSms.Views.Sms
{
    public partial class SmsAdminView
    {
        SmsMessage transaction = new SmsMessage();

        [Inject]
        DataContext DataContext { get; set; } = null!;

        [Inject]
        IHubContext<OneSmsHub> HubContext { get; set; } = null!;

        [Inject]
        IServerConnectionService ServerConnectionService { get; set; } = null!;

        [Inject]
        HubEventService HubEventService { get; set; } = null!;

        protected async override Task OnInitializedAsync()
        {
            ViewModel = new ViewModels.SmsAdminViewModel(DataContext, ServerConnectionService, HubContext,HubEventService);
            await ViewModel.LoadSimCards.Execute().ToTask();
        }

        private async Task OnFinish(EditContext editContext)
        {
            transaction.MessageStatus = MessageStatus.Sending;
            transaction.StartTime = DateTime.UtcNow;
            transaction.AppId = ViewModel.SelectedSimCard.Apps.First().AppId;
            transaction.MobileServerId = ViewModel.SelectedSimCard.MobileServerId;
            await ViewModel.AddSmsTransaction.Execute(transaction).ToTask();
            transaction = new SmsMessage { Body = transaction.Body,SenderNumber =  transaction.SenderNumber, RecieverNumber = transaction.RecieverNumber, Label = transaction.Label };
        }

        private void OnSimSelectChange(OneOf<string, IEnumerable<string>, LabeledValue, IEnumerable<LabeledValue>> value, OneOf<SelectOption, IEnumerable<SelectOption>> option)
        {
            var sim = ViewModel.Sims.First(x => x.Id.ToString() == value.Value.ToString());
            ViewModel.SelectedSimCard = sim;
            transaction.SenderNumber = sim.Number;
        }
    }
}
