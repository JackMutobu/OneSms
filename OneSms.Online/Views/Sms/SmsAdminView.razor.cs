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

namespace OneSms.Online.Views.Sms
{
    public partial class SmsAdminView
    {
        SmsTransactionDto smsTransactionDto = new SmsTransactionDto();

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
            smsTransactionDto.SimSlot = ViewModel.SelectedSimCard.SimSlot;
            smsTransactionDto.TransactionState = MessageTransactionState.Sending;
            smsTransactionDto.TimeStamp = DateTime.UtcNow;
            smsTransactionDto.AppId = ViewModel.SelectedSimCard.Apps.First().AppId;
            smsTransactionDto.MobileServerId = ViewModel.SelectedSimCard.MobileServerId;
            var smsTransaction = new SmsTransaction(smsTransactionDto);
            ViewModel.LatestTransaction = smsTransactionDto;
            await ViewModel.AddSmsTransaction.Execute(smsTransaction).ToTask();
        }

        private void OnSimSelectChange(OneOf<string, IEnumerable<string>, LabeledValue, IEnumerable<LabeledValue>> value, OneOf<SelectOption, IEnumerable<SelectOption>> option)
        {
            var sim = ViewModel.Sims.First(x => x.Id == int.Parse(value.Value.ToString()));
            ViewModel.SelectedSimCard = sim;
            smsTransactionDto.SenderNumber = sim.Number;
        }
    }
}
