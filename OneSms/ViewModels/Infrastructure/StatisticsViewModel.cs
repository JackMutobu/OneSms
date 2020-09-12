using BlazorInputFile;
using DynamicData;
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
            Authenticate.ThrownExceptions.Select(x => x.Message).ToPropertyEx(this, x => x.Errors);
            Authenticate.IsExecuting.Do(x => IsBusy = x).Subscribe();


            SendMessages = ReactiveCommand.CreateFromTask<SendMessageRequest, List<MessageResponse>>(async messageRequest =>
             {
                 var data = JsonSerializer.Serialize(messageRequest);
                 var content = new StringContent(data, Encoding.UTF8, "application/json");
                 var response = await _httpClient.PostAsync("/api/v1/whatsapp/send", content);
                 if(response.IsSuccessStatusCode)
                 {
                     var sendMessageResponse = JsonSerializer.Deserialize<SendMessageResponse>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                     var getMessages = await _httpClient.GetAsync($"/api/v1/whatsapp/transaction/{sendMessageResponse.TransactionId}");
                     var messageString = await getMessages.Content.ReadAsStringAsync();
                     var result = JsonSerializer.Deserialize<List<MessageResponse>>(messageString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                     return result;
                 }
                 else
                     throw new Exception(await response.Content.ReadAsStringAsync());
             });
            SendMessages.Do(messages => Messages.AddRange(messages)).Subscribe();
            SendMessages.IsExecuting.Do(x => IsBusy = x).Subscribe();
            SendMessages.ThrownExceptions.Select(x => x.Message).ToPropertyEx(this, x => x.Errors);

            _hubEventService.OnWhatsappMessageStatusChanged.ObserveOn(RxApp.MainThreadScheduler).Subscribe(message =>
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
            UploadFile.ThrownExceptions.Select(x => x.Message).ToPropertyEx(this, x => x.Errors);
            UploadFile.IsExecuting.Do(x => IsBusy = x).Subscribe();
        }

        public string? Errors { [ObservableAsProperty]get; }

        public string? AuthMessage { [ObservableAsProperty]get; }

        [Reactive]
        public bool IsBusy { get; set; }

        public ReactiveCommand<ApiAuthRequest, AuthSuccessResponse> Authenticate { get; }

        public ReactiveCommand<SendMessageRequest, List<MessageResponse>> SendMessages { get; }

        public ReactiveCommand<IFileListEntry, FileUploadSuccessReponse> UploadFile { get; }

        [Reactive]
        public ObservableCollection<MessageResponse> Messages { get; set; } = new ObservableCollection<MessageResponse>();

        [Reactive]
        public string? AppId { get; set; }

        [Reactive]
        public string? ImageLink { get; set; }
    }
}
