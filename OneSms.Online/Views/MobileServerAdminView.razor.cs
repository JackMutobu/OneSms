using AntDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using OneOf;
using OneSms.Online.Data;
using OneSms.Online.ViewModels;
using OneSms.Web.Shared.Models;
using System;
using System.Collections.Generic;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace OneSms.Online.Views
{
    public partial class MobileServerAdminView
    {
        bool modalVisible = false;
        ServerMobile serverMobile = new ServerMobile();

        [Inject]
        OneSmsDbContext OneSmsDbContext { get; set; }

        protected async override Task OnInitializedAsync()
        {
            base.OnInitialized();
            ViewModel = new MobileServerAdminViewModel(OneSmsDbContext);
            await ViewModel.LoadServerMobiles.Execute().ToTask();
            await ViewModel.LoadSimCards.Execute().ToTask();
        }

        private async Task AddOrUpdate(ServerMobile serverMobile)
            => await ViewModel.AddOrUpdateServerMobile.Execute(serverMobile).ToTask();

        private async Task Delete(ServerMobile serverMobile)
            => await ViewModel.DeleteServerMobile.Execute(serverMobile).ToTask();

        private async Task Save(EditContext editContext)
        {
            modalVisible = false;
            await AddOrUpdate(serverMobile);
            serverMobile = new ServerMobile();
        }
        private void ShowModal(ServerMobile server)
        {
            modalVisible = true;
            if (server.Key == Guid.Empty)
                serverMobile.Key = Guid.NewGuid();

            serverMobile = server ?? new ServerMobile();
        }
        private void HideModal() => modalVisible = false;

        private void OnSimOneChange(OneOf<string, IEnumerable<string>, LabeledValue, IEnumerable<LabeledValue>> value, OneOf<SelectOption, IEnumerable<SelectOption>> option)
        {
            serverMobile.SimOneId = int.Parse(value.Value.ToString());
        }
        private void OnSimTwoChange(OneOf<string, IEnumerable<string>, LabeledValue, IEnumerable<LabeledValue>> value, OneOf<SelectOption, IEnumerable<SelectOption>> option)
        {
            serverMobile.SimTwoId = int.Parse(value.Value.ToString());
        }
    }
}
