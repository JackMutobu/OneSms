using AntDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using OneOf;
using OneSms.Constants;
using OneSms.Data;
using OneSms.Domain;
using OneSms.Services;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace OneSms.Views.Sms
{
    public partial class SmsTransactionView
    {
        [Inject]
        DataContext DataContext { get; set; } = null!;

        [Inject]
        ISmsService SmsService { get; set; } = null!;

        [Inject]
        AuthenticationStateProvider AuthenticationsStateProvider { get; set; } = null!;

        protected async override Task OnInitializedAsync()
        {
            ViewModel = new ViewModels.SmsTransactionViewModel(DataContext, SmsService);
            var authstate = await AuthenticationsStateProvider.GetAuthenticationStateAsync();
            var user = authstate.User;
            var userId = user.Claims.First(x => x.Type == "id").Value;
            await ViewModel.LoadApps.Execute(user.IsInRole(Roles.SuperAdmin) ? "" : userId).ToTask();
        }
        private async Task Delete(SmsMessage message)
            => await ViewModel.DeleteTransaction.Execute(message).ToTask();

        private void OnAppSelectionChange(OneOf<string, IEnumerable<string>, LabeledValue, IEnumerable<LabeledValue>> value, OneOf<SelectOption, IEnumerable<SelectOption>> option)
        {
            ViewModel.SelectedApp = ViewModel.Apps.Single(x => x.Id.ToString() == value.AsT0.ToString());
        }
    }
}
