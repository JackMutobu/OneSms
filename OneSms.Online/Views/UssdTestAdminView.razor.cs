using AntDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.SignalR;
using OneOf;
using OneSms.Online.Data;
using OneSms.Online.Hubs;
using OneSms.Online.Services;
using OneSms.Web.Shared.Dtos;
using OneSms.Web.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace OneSms.Online.Views
{
    public partial class UssdTestAdminView
    {
        UssdTransactionDto ussdTransactionDto = new UssdTransactionDto();

        [Inject]
        OneSmsDbContext OneSmsDbContext { get; set; }

        [Inject]
        IHubContext<OneSmsHub> OneSmsHubContext { get; set; }

        [Inject]
        HubEventService HubEventService { get; set; }

        [Inject]
        ServerConnectionService ServerConnectionService { get; set; }

        protected async override Task OnInitializedAsync()
        {
            ViewModel = new ViewModels.UssdTestAdminViewModel(OneSmsDbContext,HubEventService,ServerConnectionService, OneSmsHubContext);
            await ViewModel.LoadMobileServers.Execute().ToTask();
        }

        private void OnSimSelectChange(OneOf<string, IEnumerable<string>, LabeledValue, IEnumerable<LabeledValue>> value, OneOf<SelectOption, IEnumerable<SelectOption>> option)
        {
            var sim = ViewModel.SimCards.First(x => x.Id == int.Parse(value.Value.ToString()));
            ViewModel.SelectedSimCard = sim;
            ussdTransactionDto.SimId = sim.Id;
            ussdTransactionDto.SimSlot = sim.SimSlot;
        }
        private void OnUssdActionSelectChange(OneOf<string, IEnumerable<string>, LabeledValue, IEnumerable<LabeledValue>> value, OneOf<SelectOption, IEnumerable<SelectOption>> option)
        {
            var ussdAction = ViewModel.UssdActions.First(x => (int)x == int.Parse(value.Value.ToString()));
            ussdTransactionDto.ActionType = ussdAction;
            ViewModel.SelectedAction = ussdAction;
        }
        private void OnServerChange(OneOf<string, IEnumerable<string>, LabeledValue, IEnumerable<LabeledValue>> value, OneOf<SelectOption, IEnumerable<SelectOption>> option)
        {
            ViewModel.CurrentServerKey = value.Value.ToString();
        }
        private async Task CancelUssdOperation(string serverKey)
        {
            await ViewModel.CancelUssdSession.Execute(serverKey).ToTask();
        }
        private async Task OnFinish(EditContext editContext)
        {
            await ViewModel.AddUssdTransaction.Execute(ussdTransactionDto).ToTask();
        }
    }
}
