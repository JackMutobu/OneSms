using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Net.Http.Headers;
using OneSms.Contracts.V1;
using OneSms.Contracts.V1.Enumerations;
using OneSms.Contracts.V1.Requests;
using OneSms.Contracts.V1.Responses;
using OneSms.Domain;
using OneSms.Hubs;
using OneSms.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OneSms.Controllers.V1
{
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class MessagesController: ControllerBase
    {
        private readonly ISmsService _smsService;
        private readonly IWhatsappService _whatsappService;
        private readonly IMapper _mapper;
        private readonly IUriService _uriService;
        private readonly IHubContext<OneSmsHub> _hubContext;
        private readonly IServerConnectionService _serverConnectionService;
        private readonly IHttpClientFactory _clientFactory;

        public MessagesController(ISmsService smsService, IWhatsappService whatsappService, IMapper mapper,IUriService uriService,
            IHubContext<OneSmsHub> hubContext, IServerConnectionService serverConnectionService, IHttpClientFactory clientFactory)
        {
            _smsService = smsService;
            _whatsappService = whatsappService;
            _mapper = mapper;
            _uriService = uriService;
            _hubContext = hubContext;
            _serverConnectionService = serverConnectionService;
            _clientFactory = clientFactory;
        }

        [HttpPost(ApiRoutes.Message.Send)]
        public async Task<IActionResult> SendMessage(SendMessageRequest messageRequest)
        {
            var numberOfSentMessages = 0;
            var numberOfPendingMessages = 0;
            var transactionId = Guid.NewGuid().ToString();
            foreach (var processor in messageRequest.Processors)
            {
                switch (processor)
                {
                    case MessageProcessor.SMS:
                        var (sentMessages, pendingMessages) = await SendSms(_smsService.RegisterSendMessageRequest(messageRequest, transactionId));
                        numberOfPendingMessages += pendingMessages;
                        numberOfSentMessages += sentMessages;
                        break;
                    case MessageProcessor.Whatsapp:
                        var whatsappResult = await SendWhatsapp(_whatsappService.RegisterSendMessageRequest(messageRequest, transactionId));
                        numberOfPendingMessages += whatsappResult.pendingMessages;
                        numberOfSentMessages += whatsappResult.sentMessages;
                        break;
                }
            }
            return Created(_uriService.GetMessageByTransactionId(ApiRoutes.Message.Controller, transactionId), new SendMessageResponse
            {
                SentMessages = numberOfSentMessages,
                PendingMessages = numberOfPendingMessages,
                TransactionId = transactionId.ToString()
            });
        }

        [HttpPost(ApiRoutes.Message.SendPending)]
        public async Task<IActionResult> SendPendingMessage(string serverId)
        {
            string? serverConnectionId;
            if(_serverConnectionService.ConnectedServers.TryGetValue(serverId, out serverConnectionId))
                await _hubContext.Clients.Client(serverConnectionId).SendAsync(SignalRKeys.ResetToActive);

            var numberOfSentMessages = 0;
            var numberOfPendingMessages = 0;
            foreach(var message in await _whatsappService.GetListOfPendingMessages(serverId))
            {
                var (sentMessages, pendingMessages) = await OnSendWhatsapp(message);
                numberOfPendingMessages += pendingMessages;
                numberOfSentMessages += sentMessages;
            }

            foreach (var message in await _smsService.GetListOfPendingMessages(serverId))
            {
                var (sentMessages, pendingMessages) = await OnSendSms(message);
                numberOfPendingMessages += pendingMessages;
                numberOfSentMessages += sentMessages;
            }
            return Ok(new SendMessageResponse
            {
                PendingMessages = numberOfPendingMessages,
                SentMessages = numberOfSentMessages
            });
        }

        [HttpGet(ApiRoutes.Message.GetAllByTransactionId)]
        public async Task<IActionResult> GetMessagesByTransactionId(string transactionId)
        {
            var messages = new List<MessageResponse>();

            await foreach (var message in _smsService.GetMessagesByTransactionId(new Guid(transactionId)))
            {
                var sms = _mapper.Map<MessageResponse>(message);
                messages.Add(sms);
            }

            await foreach (var message in _whatsappService.GetMessagesByTransactionId(new Guid(transactionId)))
            {
                var whatsapp = _mapper.Map<MessageResponse>(message);
                messages.Add(whatsapp);
            }

            return Ok(messages);
        }

        [HttpGet(ApiRoutes.Message.GetAllByAppId)]
        public async Task<IActionResult> GetMessagesByAppId(string appId)
        {
            var messages = new List<MessageResponse>();

            await foreach (var message in _smsService.GetMessages(new Guid(appId)))
            {
                var sms = _mapper.Map<MessageResponse>(message);
                messages.Add(sms);
            }

            await foreach (var message in _whatsappService.GetMessages(new Guid(appId)))
            {
                var whatsapp = _mapper.Map<MessageResponse>(message);
                messages.Add(whatsapp);
            }

            return Ok(messages);
        }

        private async Task<(int sentMessages, int pendingMessages)> SendSms(IAsyncEnumerable<SmsMessage> messages)
        {
            int sentMessages = 0;
            int pendingMessages = 0;
            await foreach (var sms in messages)
            {
                var result = await OnSendSms(sms);
                sentMessages += result.sentMessages;
                pendingMessages += result.pendingMessages;
            }

            return (sentMessages, pendingMessages);
        }

        private async Task<(int sentMessages, int pendingMessages)> SendWhatsapp(IAsyncEnumerable<WhatsappMessage> messages)
        {
            int sentMessages = 0;
            int pendingMessages = 0;
            await foreach (var message in messages)
            {
                var result = await OnSendWhatsapp(message);
                sentMessages += result.sentMessages;
                pendingMessages += result.pendingMessages;
            }

            return (sentMessages, pendingMessages);
        }

        private async Task<(int sentMessages, int pendingMessages)> OnSendWhatsapp(WhatsappMessage message)
        {
            string? serverConnectionId;
            int sentMessages = 0;
            int pendingMessages = 0;
            ++pendingMessages;
            if (_serverConnectionService.ConnectedServers.TryGetValue(message.MobileServerId.ToString(), out serverConnectionId))
            {
                var request = await _whatsappService.OnSendingMessage(message);
                await _hubContext.Clients.Client(serverConnectionId).SendAsync(SignalRKeys.SendWhatsapp, request);
                ++sentMessages;
                --pendingMessages;
                ShareContact(message.AppId.ToString(), message.RecieverNumber, serverConnectionId);
            }
            return (sentMessages, pendingMessages);
        }

        private async Task<(int sentMessages, int pendingMessages)> OnSendSms(SmsMessage message)
        {
            string? serverConnectionId;
            int sentMessages = 0;
            int pendingMessages = 0;
            ++pendingMessages;
            if (_serverConnectionService.ConnectedServers.TryGetValue(message.MobileServerId.ToString(), out serverConnectionId))
            {
                var request = await _smsService.OnSendingMessage(message);
                await _hubContext.Clients.Client(serverConnectionId).SendAsync(SignalRKeys.SendSms, request);
                ++sentMessages;
                --pendingMessages;
            }
            return (sentMessages, pendingMessages);
        }

        private Task ShareContact(string appId, string number, string serverConnectionId)
        {
            var bearerToken = HttpContext.Request.Headers[HeaderNames.Authorization].FirstOrDefault(x => x.Contains("Bearer"))?.Replace("Bearer ", "");
            if (!string.IsNullOrEmpty(bearerToken))
            {
                var client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
                var request = new HttpRequestMessage(HttpMethod.Post, $"{_uriService.InternetUrl}/{ApiRoutes.Contact.Share}");
                var shareContactRequest = new SharingContactRequest
                {
                    AppId = new Guid(appId),
                    Number = number,
                    ServerConnectionId = serverConnectionId
                };
                HttpContent httpContent = new StringContent(JsonSerializer.Serialize(shareContactRequest),Encoding.UTF8, "application/json");
                request.Content = httpContent;
                client.SendAsync(request);
            }
            return Task.CompletedTask;
        }
    }
}
