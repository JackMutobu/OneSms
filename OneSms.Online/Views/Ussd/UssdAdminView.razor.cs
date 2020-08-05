using AntDesign;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using OneOf;
using OneSms.Online.Data;
using OneSms.Online.ViewModels;
using OneSms.Web.Shared.Enumerations;
using OneSms.Web.Shared.Models;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace OneSms.Online.Views.Ussd
{
    public partial class UssdAdminView
    {
        UssdAction ussdAction = new UssdAction();
        bool ussdModalVisible = false;
        UssdActionStep ussdActionStep = new UssdActionStep();
        bool ussdModalActionStep = false;
        bool ussdModalEditActionStep = false;

        [Inject]
        OneSmsDbContext OneSmsDbContext { get; set; }

        protected async override Task OnInitializedAsync()
        {
            base.OnInitialized();
            ViewModel = new UssdAdminViewModel(OneSmsDbContext);
            await ViewModel.LoadNetworks.Execute().ToTask();
            await LoadUssdItems(-1, -1);
        }
        private async void OnNetworkChange(OneOf<string, IEnumerable<string>, LabeledValue, IEnumerable<LabeledValue>> value, OneOf<SelectOption, IEnumerable<SelectOption>> option)
        {
            ViewModel.CurrentNetwork = value.AsT0.ToString() == "-1" ? null : ViewModel.Networks.Single(x => x.Id == int.Parse(value.AsT0.ToString()));
            await LoadUssdItems((int?)ViewModel.CurrentNetwork?.Id ?? -1, (int?)ViewModel.CurrentActionType ?? -1);
        }
        private void OnNetworkModalChange(OneOf<string, IEnumerable<string>, LabeledValue, IEnumerable<LabeledValue>> value, OneOf<SelectOption, IEnumerable<SelectOption>> option)
        {
            ussdAction.NetworkId = int.Parse(value.AsT0.ToString());
        }
        private async void OnUssdActionTypeChange(OneOf<string, IEnumerable<string>, LabeledValue, IEnumerable<LabeledValue>> value, OneOf<SelectOption, IEnumerable<SelectOption>> option)
        {
            ViewModel.CurrentActionType = value.AsT0.ToString() == "-1" ? null : (UssdActionType?)ViewModel.UssdActionTypes.Single(x => (int)x == int.Parse(value.AsT0.ToString()));
            await LoadUssdItems((int?)ViewModel.CurrentNetwork?.Id ?? -1, (int?)ViewModel.CurrentActionType ?? -1);
        }
        private void OnUssdActionModalTypeChange(OneOf<string, IEnumerable<string>, LabeledValue, IEnumerable<LabeledValue>> value, OneOf<SelectOption, IEnumerable<SelectOption>> option)
        {
            ussdAction.ActionType = ViewModel.UssdActionTypes[int.Parse(value.AsT0.ToString())];
        }

        private async Task LoadUssdItems(int networkId,int ussdAction) 
            => await ViewModel.LoadUssdActions.Execute((networkId, ussdAction)).ToTask();

        private async Task AddOrUpdate(UssdAction ussdAction)
           => await ViewModel.AddOrUpdateUssd.Execute(ussdAction).ToTask();

        private async Task DeleteUssd(UssdAction ussdAction)
            => await ViewModel.DeleteUssd.Execute(ussdAction).ToTask();

        private void HideModal() => ussdModalVisible = ussdModalActionStep = ussdModalEditActionStep = false;

        private async Task Save(EditContext editContext)
        {
            ussdModalVisible = false;
            if (ussdAction.NetworkId == 0)
                ussdAction.NetworkId = ViewModel.Networks.First().Id;
            await AddOrUpdate(ussdAction);
            ussdAction = new UssdAction();
        }

        private void ShowUssdModal(UssdAction ussd)
        {
            ussdModalVisible = true;
            ussdAction = ussd ?? new UssdAction();
        }

        private void ShowUssdStepModal(UssdAction ussd)
        {
            ussdModalActionStep = true;
            ViewModel.CurrentUssdAction = ussd;
        }
        private void ShowUssdStepEditModal(UssdActionStep ussd)
        {
            ussdModalActionStep = false;
            ussdModalEditActionStep = true;
            ussdActionStep = ussd ?? new UssdActionStep();
        }
        private async Task SaveUssdStep(EditContext editContext)
        {
            ussdModalEditActionStep = false;
            ussdModalActionStep = true;
            ussdActionStep.UssdActionId = ViewModel.CurrentUssdAction.Id;
            await ViewModel.AddOrUpdateUssdStep.Execute(ussdActionStep).ToTask();
            ussdActionStep = new UssdActionStep();
        }
        private async Task DeleteUssdStep(UssdActionStep ussd)
            => await ViewModel.DeleteUssdStep.Execute(ussd).ToTask();
    }
}

