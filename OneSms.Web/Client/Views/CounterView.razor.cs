using OneSms.Web.Client.ViewModels;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace OneSms.Web.Client.Views
{
    public partial class CounterView
    {
        public CounterView()
        {
            ViewModel = new CounterViewModel();
        }

        private async Task IncrementCount()
        {
            await ViewModel.Increment.Execute().ToTask();
        }
    }
}
