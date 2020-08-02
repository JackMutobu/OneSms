using AntDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using OneOf;
using OneSms.Online.Data;
using OneSms.Online.Models;
using OneSms.Online.ViewModels;
using OneSms.Web.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace OneSms.Online.Views
{
    public partial class SimAdminView
    {
        bool modalVisible = false;
        bool modalAppsVisible = false;
        bool isUpdate = false;
        bool canAddApp = false;
        SimAdminDto sim = new SimAdminDto();
        string[] defaultAppIds = default;
        List<OneSmsApp> apps = new List<OneSmsApp>();
        [Inject]
        OneSmsDbContext OneSmsDbContext { get; set; }

        protected async override Task OnInitializedAsync()
        {
            base.OnInitialized();
            ViewModel = new SimAdminViewModel(OneSmsDbContext);
            await ViewModel.LoadNetworks.Execute().ToTask();
        }
        private async Task AddOrUpdate(SimCard sim)
           => await ViewModel.AddOrUpdateCard.Execute(sim).ToTask();
        private async Task Delete(SimCard sim)
           => await ViewModel.DeleteCard.Execute(sim).ToTask();

        private async Task Save(EditContext editContext)
        {
            modalVisible = false;
            modalAppsVisible = false;
            canAddApp = false;
            var simCard = sim.GetSimCard();
            if (!isUpdate)
            {
                sim.AirtimeBalance = "0";
                sim.SmsBalance = "0";
                sim.MobileMoneyBalance = "0";
                sim.AppIds.ForEach(x =>
                {
                    if (!simCard.Apps.Any(y => y.AppId == x))
                    {

                        var appSim = new AppSim
                        {
                            SimId = sim.Id,
                            AppId = x
                        };
                        simCard.Apps.Add(appSim);
                    }
                });
            }
            else
            {
                var idsToAdd = sim.AppIds.Except(simCard.Apps.Select(x => x.AppId));
                var appSimsToAdd = new List<AppSim>();
                var idsToRemove = simCard.Apps.Select(x => x.AppId).Except(sim.AppIds);
                var appSimsToRemove = new List<AppSim>();
                foreach (var item in idsToAdd)
                {
                    var appSim = new AppSim
                    {
                        SimId = simCard.Id,
                        AppId = item
                    };
                    appSimsToAdd.Add(appSim);
                }
               
                foreach (var item in idsToRemove)
                {
                    var appSim = new AppSim
                    {
                        SimId = simCard.Id,
                        AppId = item
                    };
                    appSimsToRemove.Add(appSim);
                };
                await ViewModel.AddAppSim.Execute(appSimsToAdd).ToTask();
                await ViewModel.DeleteAppSim.Execute(appSimsToRemove).ToTask();
                simCard.Apps = null;
            }
           
            await AddOrUpdate(simCard);
            sim = new SimAdminDto();
            apps = new List<OneSmsApp>();
        }
        private void ShowModal(SimCard simCard)
        {
            modalVisible = true;
            isUpdate = simCard.NetworkId != 0;
            if (simCard != null)
            {
                sim = new SimAdminDto(simCard);
                defaultAppIds = simCard.Apps.Select(x => x.AppId.ToString()).ToArray();
            }
        }
        private void ShowAppsModal(SimCard simCard)
        {
            modalAppsVisible = true;
            isUpdate = simCard.NetworkId != 0;
            if (simCard != null)
            {
                sim = new SimAdminDto(simCard);
                var appIds = simCard.Apps.Select(x => x.AppId);
                appIds.ForEach(id => apps.Add(ViewModel.Apps.First(x => x.AppId == id)));
                if (sim.Id == 5)
                    defaultAppIds = null;
            }
        }
        private void HideModal()
        {
            modalVisible = modalAppsVisible = false;
            apps = new List<OneSmsApp>();
        }

        private  void OnNetworkChange(OneOf<string, IEnumerable<string>, LabeledValue, IEnumerable<LabeledValue>> value, OneOf<SelectOption, IEnumerable<SelectOption>> option)
        {
            sim.NetworkId = ViewModel.Networks.Single(x => x.Id == int.Parse(value.AsT0.ToString())).Id;
        }
        private void OnAppSelectChange(OneOf<string, IEnumerable<string>, LabeledValue, IEnumerable<LabeledValue>> value, OneOf<SelectOption, IEnumerable<SelectOption>> option)
        {
            sim.AppIds = ((IEnumerable<string>)value.Value).Select(x => new Guid(x)).ToList();
        }
        private void OnServerSelectChange(OneOf<string, IEnumerable<string>, LabeledValue, IEnumerable<LabeledValue>> value, OneOf<SelectOption, IEnumerable<SelectOption>> option)
        {
            sim.MobileServerId = ViewModel.MobileServers.First(x => x.Id == int.Parse(value.Value.ToString())).Id;
        }
        private async Task OnRemoveApp(OneSmsApp oneSmsApp)
        {
            apps.Remove(oneSmsApp);
            await ViewModel.DeleteAppSim.Execute(new List<AppSim> { new AppSim { AppId = oneSmsApp.AppId, SimId = sim.Id } }).ToTask();
        }
        private void OnAppModalSelectChange(OneOf<string, IEnumerable<string>, LabeledValue, IEnumerable<LabeledValue>> value, OneOf<SelectOption, IEnumerable<SelectOption>> option)
        {
            sim.AppIds.AddRange(((IEnumerable<string>)value.Value).Select(x => new Guid(x)));
        }
        private void CanAddApp() => canAddApp = !canAddApp;
    }
}
