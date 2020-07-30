using Microsoft.AspNetCore.Components;
using OneSms.Web.Client.ViewModels;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace OneSms.Web.Client.Views
{
    public partial class FetchDataView
    {
        [Inject]
        public FetchDataViewModel FetchViewModel
        {
            get => ViewModel;
            set => ViewModel = value;

        }

        protected override async Task OnInitializedAsync()
        {
            //await ViewModel.LoadForecasts.Execute().ToTask();
        }
    }
}
