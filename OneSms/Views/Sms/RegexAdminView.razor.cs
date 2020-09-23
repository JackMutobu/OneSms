using AntDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using OneOf;
using OneSms.Data;
using OneSms.Domain;
using OneSms.Services;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace OneSms.Views.Sms
{
    public partial class RegexAdminView
    {
        bool modalVisible = false;
        NetworkMessageExtractor networkMessageExtractor = new NetworkMessageExtractor();

        [Inject]
        IMessageExtractionService MessageExtractionService { get; set; } = null!;

        [Inject]
        INetworkService NetworkService { get; set; } = null!;

        protected async override Task OnInitializedAsync()
        {
            base.OnInitialized();
            ViewModel = new ViewModels.RegexAdminViewModel(MessageExtractionService, NetworkService);
            await ViewModel.LoadNetworks.Execute().ToTask();
        }

        private async Task AddOrUpdate(NetworkMessageExtractor item)
            => await ViewModel.AddOrUpdateItem.Execute(item).ToTask();

        private async Task Delete(NetworkMessageExtractor item)
            => await ViewModel.DeleteItem.Execute(item).ToTask();

        private async Task Save(EditContext editContext)
        {
            modalVisible = false;
            await AddOrUpdate(networkMessageExtractor);
            networkMessageExtractor = new NetworkMessageExtractor();
        }
        private void ShowModal(NetworkMessageExtractor item)
        {
            modalVisible = true;
            networkMessageExtractor = item ?? new NetworkMessageExtractor();
        }
        private void HideModal() => modalVisible = false;
        private void OnNetworkChange(OneOf<string, IEnumerable<string>, LabeledValue, IEnumerable<LabeledValue>> value, OneOf<SelectOption, IEnumerable<SelectOption>> option)
        {
            networkMessageExtractor.NetworkId = int.Parse(value.AsT0.ToString());
        }
        private  void OnUssdActionTypeChange(OneOf<string, IEnumerable<string>, LabeledValue, IEnumerable<LabeledValue>> value, OneOf<SelectOption, IEnumerable<SelectOption>> option)
        {
            networkMessageExtractor.NetworkAction = ViewModel.NetworkActionTypes.Single(x => (int)x == int.Parse(value.AsT0.ToString()));
        }
    }
}
