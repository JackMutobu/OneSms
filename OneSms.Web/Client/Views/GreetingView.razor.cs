using OneSms.Web.Client.ViewModels;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace OneSms.Web.Client.Views
{
    public partial class GreetingView
    {
        public GreetingView()
        {
            ViewModel = new GreetingViewModel();
        }

        public async Task Clear()
        {
            await ViewModel.Clear.Execute().ToTask();
        }
    }
}
