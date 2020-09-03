using AntDesign;
using BlazorInputFile;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using OneOf;
using OneSms.Data;
using OneSms.Hubs;
using OneSms.Online.Services;
using OneSms.Services;
using OneSms.Web.Shared.Enumerations;
using OneSms.Web.Shared.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace OneSms.Views.Whatsapp
{
    public partial class WhatsappAdminView
    {
        WhatsappTransaction transaction = new WhatsappTransaction();
        IFileListEntry fileImage;
        string imageUrl;

        [Inject]
        OneSmsDbContext OneSmsDbContext { get; set; }

        [Inject]
        IHubContext<OneSmsHub> OneSmsHubContext { get; set; }

        [Inject]
        ServerConnectionService ServerConnectionService { get; set; }

        [Inject]
        HubEventService HubEventService { get; set; }
        [Inject]
        IWebHostEnvironment Environment { get; set; }
        [Inject]
        NavigationManager UriHelper { get; set; }

        protected async override Task OnInitializedAsync()
        {
            ViewModel = new ViewModels.Whatsapp.WhatsappAdminViewModel(OneSmsDbContext, ServerConnectionService, OneSmsHubContext, HubEventService);
            await ViewModel.LoadMobileServers.Execute().ToTask();
        }

        private async Task OnFinish(EditContext editContext)
        {
            var numbers = transaction.RecieverNumber;
            var recipients = transaction.RecieverNumber.Split(",");
            transaction.TransactionState = MessageTransactionState.Sending;
            transaction.MobileServerId = ViewModel.MobileServer.Id;
            transaction.ImageLinkOne = imageUrl;
            foreach (var number in recipients)
            {
                transaction.RecieverNumber = number;
                await ViewModel.AddTransaction.Execute(transaction).ToTask();
                transaction = new WhatsappTransaction() { RecieverNumber = transaction.RecieverNumber, Title = transaction.Title, Body = transaction.Body, MobileServerId = transaction.MobileServerId, ImageLinkOne = imageUrl,TransactionState = transaction.TransactionState};
            }
            transaction.RecieverNumber = numbers;
            
        }

        private void OnServerSelectChange(OneOf<string, IEnumerable<string>, LabeledValue, IEnumerable<LabeledValue>> value, OneOf<SelectOption, IEnumerable<SelectOption>> option)
        {
            var server = ViewModel.MobileServers.First(x => x.Id == int.Parse(value.Value.ToString()));
            ViewModel.MobileServer = server;
        }

        async Task HandleFileSelected(IFileListEntry[] files)
        {
            fileImage = files.FirstOrDefault();
            imageUrl = await UploadImage(fileImage);
        }

        public async Task<string> UploadImage(IFileListEntry file)
        {
            var profileUploads = Path.Combine(Environment.WebRootPath, $"uploads/images");
            if (!Directory.Exists(profileUploads))
                Directory.CreateDirectory(profileUploads);
            if (file.Data.Length > 0)
            {
                var fileNameArray = file.Name.Split(".");
                var fileName = fileNameArray.FirstOrDefault() + $".{fileNameArray.LastOrDefault() ?? "png"}";
                using var fileStream = new FileStream(Path.Combine(profileUploads, fileName), FileMode.OpenOrCreate);
                await file.Data.CopyToAsync(fileStream);
                var baseUrl = UriHelper.BaseUri.Substring(0, UriHelper.BaseUri.Length - 1);
                if (baseUrl.Contains("localhost"))
                    baseUrl = "https://e69b70a3ee5d.ngrok.io";
                var filePath = fileStream.Name.Replace(Environment.WebRootPath, baseUrl);
                return filePath.Replace("\\", "/");
            }
            return string.Empty;
        }
    }
}
