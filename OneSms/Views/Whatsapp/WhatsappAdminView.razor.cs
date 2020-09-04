using AntDesign;
using BlazorInputFile;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using OneOf;
using OneSms.Contracts.V1.Enumerations;
using OneSms.Data;
using OneSms.Domain;
using OneSms.Hubs;
using OneSms.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace OneSms.Views.Whatsapp
{
    public partial class WhatsappAdminView
    {
        WhatsappMessage transaction = new WhatsappMessage();
        IFileListEntry? fileImage;
        string? imageUrl;

        [Inject]
        DataContext DataContext { get; set; } = null!;

        [Inject]
        IHubContext<OneSmsHub> HubContext { get; set; } = null!;

        [Inject]
        IServerConnectionService ServerConnectionService { get; set; } = null!;

        [Inject]
        HubEventService HubEventService { get; set; } = null!;
        [Inject]
        IWebHostEnvironment Environment { get; set; } = null!;
        [Inject]
        NavigationManager UriHelper { get; set; } = null!;
        [Inject]
        IUriService UriService { get; set; } = null!;

        protected async override Task OnInitializedAsync()
        {
            ViewModel = new ViewModels.Whatsapp.WhatsappAdminViewModel(DataContext, ServerConnectionService, HubContext, HubEventService);
            await ViewModel.LoadMobileServers.Execute().ToTask();
        }

        private async Task OnFinish(EditContext editContext)
        {
            var numbers = transaction.RecieverNumber;
            var recipients = transaction.RecieverNumber.Split(",");
            transaction.MessageStatus = MessageStatus.Sending;
            transaction.MobileServerId = ViewModel.MobileServer.Id;
            transaction.ImageLinkOne = imageUrl;
            foreach (var number in recipients)
            {
                transaction.RecieverNumber = number;
                await ViewModel.AddTransaction.Execute(transaction).ToTask();
                transaction = new WhatsappMessage() { RecieverNumber = transaction.RecieverNumber, Label = transaction.Label, Body = transaction.Body, MobileServerId = transaction.MobileServerId, ImageLinkOne = imageUrl,MessageStatus = transaction.MessageStatus};
            }
            transaction.RecieverNumber = numbers;
            
        }

        private void OnServerSelectChange(OneOf<string, IEnumerable<string>, LabeledValue, IEnumerable<LabeledValue>> value, OneOf<SelectOption, IEnumerable<SelectOption>> option)
        {
            var server = ViewModel.MobileServers.First(x => x.Id.ToString() == value.Value.ToString());
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
             
                var filePath = fileStream.Name.Replace(Environment.WebRootPath, UriService.InternetUrl);
                return filePath.Replace("\\", "/");
            }
            return string.Empty;
        }
    }
}
