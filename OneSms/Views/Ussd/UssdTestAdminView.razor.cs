using AntDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.SignalR;
using OneOf;
using OneSms.Data;
using OneSms.Domain;
using OneSms.Hubs;
using OneSms.Services;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace OneSms.Views.Ussd
{
    public partial class UssdTestAdminView
    {
        UssdTransaction ussdTransaction = new UssdTransaction();

        [Inject]
        DataContext DataContext { get; set; } = null!;

        [Inject]
        IHubContext<OneSmsHub> HubContext { get; set; } = null!;

        [Inject]
        HubEventService HubEventService { get; set; } = null!;

        [Inject]
        ServerConnectionService ServerConnectionService { get; set; } = null!;

        protected async override Task OnInitializedAsync()
        {
            ViewModel = new ViewModels.UssdTestAdminViewModel(DataContext,HubEventService,ServerConnectionService, HubContext);
            await ViewModel.LoadMobileServers.Execute().ToTask();
        }

        private void OnSimSelectChange(OneOf<string, IEnumerable<string>, LabeledValue, IEnumerable<LabeledValue>> value, OneOf<SelectOption, IEnumerable<SelectOption>> option)
        {
            var sim = ViewModel.SimCards.First(x => x.Id == int.Parse(value.Value.ToString()));
            ViewModel.SelectedSimCard = sim;
            ussdTransaction.SimId = sim.Id;
        }
        private void OnUssdActionSelectChange(OneOf<string, IEnumerable<string>, LabeledValue, IEnumerable<LabeledValue>> value, OneOf<SelectOption, IEnumerable<SelectOption>> option)
        {
            var ussdAction = ViewModel.UssdActions.First(x => x.Id == int.Parse(value.Value.ToString()));
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
            await ViewModel.AddUssdTransaction.Execute(ussdTransaction).ToTask();
        }
    }
}
