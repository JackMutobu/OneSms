using BlazorInputFile;
using DynamicData;
using OneSms.Contracts.V1;
using OneSms.Contracts.V1.Requests;
using OneSms.Contracts.V1.Responses;
using OneSms.Data;
using OneSms.Domain;
using OneSms.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OneSms.ViewModels.Infrastructure
{
    public class StatisticsViewModel: ReactiveObject
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpClient _httpClient;
        private readonly IUriService _uriService;
        private readonly HubEventService _hubEventService;

        public StatisticsViewModel(IHttpClientFactory httpClientFactory,IUriService uriService, HubEventService hubEventService)
        {
            _httpClientFactory = httpClientFactory;
            _httpClient = _httpClientFactory.CreateClient();
            _uriService = uriService;
            _hubEventService = hubEventService;
            _httpClient.BaseAddress = new System.Uri(_uriService.InternetUrl);
            AuthMessage = "Not authenticated";
            Authenticate = ReactiveCommand.CreateFromTask<ApiAuthRequest, AuthSuccessResponse>(async requestObject =>
             {
                 var data = JsonSerializer.Serialize(requestObject);
                 var content = new StringContent(data, Encoding.UTF8, "application/json");
                 var response = await _httpClient.PostAsync("/api/v1/auth/app", content);
                 if (response.IsSuccessStatusCode)
                     return JsonSerializer.Deserialize<AuthSuccessResponse>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                 else
                     throw new Exception(await response.Content.ReadAsStringAsync());
             });
            Authenticate.Do(authData => _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authData.Token)).Subscribe();
            Authenticate.Select(_ => "Authenticated").ToPropertyEx(this, x => x.AuthMessage);
            Authenticate.ThrownExceptions.Do(x => Errors = x.Message).Subscribe();
            Authenticate.IsExecuting.Do(x => IsBusy = x).Subscribe();


            SendMessages = ReactiveCommand.CreateFromTask<SendMessageRequest, List<MessageResponse>?>(async messageRequest =>
            {
                try
                {
                    var data = JsonSerializer.Serialize(messageRequest);
                    var content = new StringContent(data, Encoding.UTF8, "application/json");
                    if (messageRequest.Processors?.FirstOrDefault() == Contracts.V1.Enumerations.MessageProcessor.Whatsapp)
                        return await SendMessage(content, ApiRoutes.Whatsapp.Send, "whatsapp");
                    else
                        return await SendMessage(content, ApiRoutes.Sms.Send, "sms");
                }
                catch(Exception ex)
                {
                    Errors = ex.Message;
                }
                return null;
            });
            SendMessages.Where(x => x != null).Do(messages => Messages.AddRange(messages)).Subscribe();
            SendMessages.IsExecuting.Do(x => IsBusy = x).Subscribe();
            SendMessages.ThrownExceptions.Do(x => Errors = x.Message).Subscribe();

            _hubEventService.OnMessageStateChanged.ObserveOn(RxApp.MainThreadScheduler).Subscribe(message =>
            {
                var item = Messages.FirstOrDefault(x => x.Id == message.Id);
                if (item != null)
                {
                    item.CompletedTime = message.CompletedTime;
                    item.MessageStatus = message.MessageStatus;
                    Messages.Remove(item);
                    Messages.Add(item);
                    Messages = new ObservableCollection<MessageResponse>(Messages.OrderByDescending(x => x.CompletedTime));
                }
            });

            UploadFile = ReactiveCommand.CreateFromTask<IFileListEntry, FileUploadSuccessReponse>(async file =>
             {
                 using var content = new MultipartFormDataContent
                 {
                     {
                         new StreamContent(file.Data)
                         {
                             Headers =
                                    {
                                        ContentLength = file.Size,
                                        ContentType = new MediaTypeHeaderValue(file.Type)
                                    }
                            },"formImage",file.Name
                     }
                 };

                 var response = await _httpClient.PostAsync("/api/v1/upload/image", content);
                 if(response.IsSuccessStatusCode)
                 {
                     var stringResult = await response.Content.ReadAsStringAsync();
                     var result = JsonSerializer.Deserialize<FileUploadSuccessReponse>(stringResult,new JsonSerializerOptions {PropertyNameCaseInsensitive = true });
                     return result;
                 }
                 else
                     throw new Exception(await response.Content.ReadAsStringAsync());
             });
            UploadFile.Do(image => ImageLink = image.Url).Subscribe();
            UploadFile.ThrownExceptions.Do(x => Errors = x.Message).Subscribe();
            UploadFile.IsExecuting.Do(x => IsBusy = x).Subscribe();
        }

        private async Task<List<MessageResponse>> SendMessage(StringContent content,string sendUrl, string transactionController)
        {
            var response = await _httpClient.PostAsync($"/{sendUrl}", content);
            if (response.IsSuccessStatusCode)
            {
                var sendMessageResponse = JsonSerializer.Deserialize<SendMessageResponse>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                var getMessages = await _httpClient.GetAsync($"/api/v1/{transactionController}/transaction/{sendMessageResponse.TransactionId}");
                var messageString = await getMessages.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<List<MessageResponse>>(messageString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result;
            }
            else
                throw new Exception(await response.Content.ReadAsStringAsync());
        }

        [Reactive]
        public string? Errors { get; set; }

        public string? AuthMessage { [ObservableAsProperty]get; }

        [Reactive]
        public bool IsBusy { get; set; }

        public ReactiveCommand<ApiAuthRequest, AuthSuccessResponse> Authenticate { get; }

        public ReactiveCommand<SendMessageRequest, List<MessageResponse>?> SendMessages { get; }

        public ReactiveCommand<IFileListEntry, FileUploadSuccessReponse> UploadFile { get; }

        [Reactive]
        public ObservableCollection<MessageResponse> Messages { get; set; } = new ObservableCollection<MessageResponse>();

        [Reactive]
        public string? AppId { get; set; }

        [Reactive]
        public string? ImageLink { get; set; }
    }
}
