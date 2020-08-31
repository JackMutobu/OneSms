using AntDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using OneOf;
using OneSms.Constants;
using OneSms.Data;
using OneSms.Domain;
using OneSms.Services;
using OneSms.ViewModels.Whatsapp;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace OneSms.Views.Whatsapp
{
    public partial class WhatsappTransactionView
    {
        [Inject]
        DataContext DataContext { get; set; } = null!;

        [Inject]
        IWhatsappService WhatsappService { get; set; } = null!;

        [Inject]
        AuthenticationStateProvider AuthenticationsStateProvider { get; set; } = null!;

        protected async override Task OnInitializedAsync()
        {
            ViewModel = new WhastappTransactionViewModel(DataContext,WhatsappService);
            var authstate = await AuthenticationsStateProvider.GetAuthenticationStateAsync();
            var user = authstate.User;
            var userId = user.Claims.First(x => x.Type == "id").Value;
            await ViewModel.LoadApps.Execute(user.IsInRole(Roles.SuperAdmin) ? "" : userId).ToTask();
        }
        private async Task Delete(WhatsappMessage transaction)
            => await ViewModel.DeleteTransaction.Execute(transaction).ToTask();

        private void OnAppSelectionChange(OneOf<string, IEnumerable<string>, LabeledValue, IEnumerable<LabeledValue>> value, OneOf<SelectOption, IEnumerable<SelectOption>> option)
        {
            ViewModel.SelectedApp = ViewModel.Apps.Single(x => x.Id.ToString() == value.AsT0.ToString());
        }
    }
}
