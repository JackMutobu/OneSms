using AntDesign;
using BlazorInputFile;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using OneOf;
using OneSms.Contracts.V1.Enumerations;
using OneSms.Contracts.V1.Requests;
using OneSms.Services;
using OneSms.Web.Shared.Constants;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace OneSms.Views.Infrastructure
{
    public partial class StatisticsView
    {
        bool modalVisible;
        ApiAuthRequest authRequest = new ApiAuthRequest();
        SendMessageRequest messageRequest = new SendMessageRequest();
        string selectedProcessor = "0";
        string? receivers;
        IFileListEntry? fileImage;

        [Inject]
        AuthenticationStateProvider AuthenticationsStateProvider { get; set; } = null!;

        [Inject]
        NavigationManager NavigationManager { get; set; } = null!;

        [Inject]
        IHttpClientFactory HttpClientFactory { get; set; } = null!;

        [Inject]
        IUriService UriService { get; set; } = null!;

        [Inject]
        HubEventService HubEventService { get; set; } = null!;

        protected override async Task OnInitializedAsync()
        {
            var authstate = await AuthenticationsStateProvider.GetAuthenticationStateAsync();
            var user = authstate.User;
            if (user.IsInRole(UserRoles.TimAccount))
                NavigationManager.NavigateTo("timtransctions");
            ViewModel = new ViewModels.Infrastructure.StatisticsViewModel(HttpClientFactory, UriService, HubEventService);
        }

        private void ShowModal(ApiAuthRequest request)
        {
            modalVisible = true;
            authRequest = request ?? new ApiAuthRequest();
        }
        private void HideModal() => modalVisible = false;

        private async Task Save(EditContext editContext)
        {
            modalVisible = false;
            ViewModel.AppId = authRequest.AppId;
            await ViewModel.Authenticate.Execute(authRequest).ToTask();
            authRequest.AppSecret = string.Empty;
        }

        async Task HandleFileSelected(IFileListEntry[] files)
        {
            fileImage = files.FirstOrDefault();
            await ViewModel.UploadFile.Execute(fileImage).ToTask();
        }

        private void Clear()
        {
            receivers = string.Empty;
            ViewModel.ImageLink = string.Empty;
            messageRequest = new SendMessageRequest();
        }

        private async Task OnSubmitMessage(EditContext editContext)
        {
            messageRequest.Recipients = receivers?.Split(",").ToList();
            messageRequest.ImageLink = string.IsNullOrEmpty(ViewModel.ImageLink) ? new List<string>() : new List<string> { ViewModel.ImageLink };
            messageRequest.AppId = string.IsNullOrEmpty(ViewModel.AppId) ? System.Guid.NewGuid() : new System.Guid(ViewModel.AppId);

            await ViewModel.SendMessages.Execute(messageRequest).ToTask();
        }

        private void OnChange(OneOf<string, IEnumerable<string>, LabeledValue, IEnumerable<LabeledValue>> value, OneOf<SelectOption, IEnumerable<SelectOption>> option)
        {
            selectedProcessor = value.AsT0.ToString();
            if (selectedProcessor == "0")
            {
                messageRequest.Processors = new List<MessageProcessor> { MessageProcessor.SMS };
            }
            else
            {
                messageRequest.Processors = new List<MessageProcessor> { MessageProcessor.Whatsapp };
            }
            StateHasChanged();
        }
    }
}
