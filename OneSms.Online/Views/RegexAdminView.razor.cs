using AntDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using OneOf;
using OneSms.Online.Data;
using OneSms.Online.Models;
using OneSms.Web.Shared.Enumerations;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace OneSms.Online.Views
{
    public partial class RegexAdminView
    {
        bool modalVisible = false;
        SmsDataExtractor smsDataExtractor = new SmsDataExtractor();

        [Inject]
        OneSmsDbContext OneSmsDbContext { get; set; }

        protected async override Task OnInitializedAsync()
        {
            base.OnInitialized();
            ViewModel = new ViewModels.RegexAdminViewModel(OneSmsDbContext);
            await ViewModel.LoadNetworks.Execute().ToTask();
        }

        private async Task AddOrUpdate(SmsDataExtractor item)
            => await ViewModel.AddOrUpdateItem.Execute(item).ToTask();

        private async Task Delete(SmsDataExtractor item)
            => await ViewModel.DeleteItem.Execute(smsDataExtractor).ToTask();

        private async Task Save(EditContext editContext)
        {
            modalVisible = false;
            await AddOrUpdate(smsDataExtractor);
            smsDataExtractor = new SmsDataExtractor();
        }
        private void ShowModal(SmsDataExtractor item)
        {
            modalVisible = true;
            smsDataExtractor = item ?? new SmsDataExtractor();
        }
        private void HideModal() => modalVisible = false;
        private void OnNetworkChange(OneOf<string, IEnumerable<string>, LabeledValue, IEnumerable<LabeledValue>> value, OneOf<SelectOption, IEnumerable<SelectOption>> option)
        {
            smsDataExtractor.NetworkId = int.Parse(value.AsT0.ToString());
        }
        private  void OnUssdActionTypeChange(OneOf<string, IEnumerable<string>, LabeledValue, IEnumerable<LabeledValue>> value, OneOf<SelectOption, IEnumerable<SelectOption>> option)
        {
            smsDataExtractor.UssdAction = ViewModel.UssdActionTypes.Single(x => (int)x == int.Parse(value.AsT0.ToString()));
        }
    }
}
