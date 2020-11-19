using AntDesign;
using BlazorInputFile;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using OneOf;
using OneSms.Contracts.V1.Enumerations;
using OneSms.Contracts.V1.Requests;
using OneSms.Services;
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
        IHttpClientFactory HttpClientFactory { get; set; } = null!;

        [Inject]
        IUriService UriService { get; set; } = null!;

        [Inject]
        HubEventService HubEventService { get; set; } = null!;

        [Inject]
        IWebHostEnvironment HostEnvironment { get; set; } = null!;

        protected override void OnInitialized()
        {
            if(HostEnvironment.IsDevelopment())
            {
                authRequest = new ApiAuthRequest
                {
                    AppId = "32d3a7e4-d18d-462d-be59-08d84cf69cca",
                    AppSecret = "7d5b118d-772a-4423-981e-4dc1a666253b"
                };
            }
            else
            {
                authRequest = new ApiAuthRequest
                {
                    AppId = "9bd40466-5714-455a-08c4-08d85670d030",
                    AppSecret = "a3fd9198-aac4-4665-b18f-d2fa2933f1bf"
                };
            }
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
