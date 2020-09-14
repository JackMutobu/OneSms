using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Net.Http.Headers;
using OneSms.Contracts.V1;
using OneSms.Contracts.V1.MobileServerRequest;
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
    public class WhatsappController : BaseMessagingController<WhatsappMessage,WhatsappRequest>
    {
        private readonly IUriService _uriService;
        private readonly IHubContext<OneSmsHub> _hubContext;
        private readonly IServerConnectionService _serverConnectionService;
        private readonly IHttpClientFactory _clientFactory;
        private List<SharingContactRequest> _shareContactRequests;

        public WhatsappController(IWhatsappService whatsappService, IMapper mapper,IUriService uriService, HubEventService hubEventService,
            IHubContext<OneSmsHub> hubContext, IServerConnectionService serverConnectionService, IHttpClientFactory clientFactory):base(whatsappService,mapper,hubEventService)
        {
            _uriService = uriService;
            _hubContext = hubContext;
            _serverConnectionService = serverConnectionService;
            _clientFactory = clientFactory;
            _shareContactRequests = new List<SharingContactRequest>();
        }

        [HttpPost(ApiRoutes.Whatsapp.Send)]
        public override Task<IActionResult> SendMessage(SendMessageRequest messageRequest)
            => base.SendMessage(messageRequest);

        [HttpGet(ApiRoutes.Whatsapp.GetAllByTransactionId)]
        public override Task<IActionResult> GetMessagesByTransactionId(string transactionId)
            => base.GetMessagesByTransactionId(transactionId);

        [HttpGet(ApiRoutes.Whatsapp.GetAllByAppId)]
        public override Task<IActionResult> GetMessagesByAppId(string appId)
            => base.GetMessagesByAppId(appId);

        [HttpPut(ApiRoutes.Whatsapp.StatusChanged)]
        public override Task<IActionResult> OnStatusChanged([FromBody] WhatsappRequest whatsappRequest)
            => base.OnStatusChanged(whatsappRequest);

        protected override  async Task<IActionResult> SendToMobileServer(SendMessageRequest messageRequest)
        {
            var numberOfSentMessages = 0;
            var numberOfPendingMessages = 0;
            var transactionId = Guid.NewGuid().ToString();

            await foreach (var whatsapp in _messagingService.RegisterSendMessageRequest(messageRequest, transactionId))
            {
                ++numberOfPendingMessages;
                if (_serverConnectionService.ConnectedServers.TryGetValue(whatsapp.MobileServerId.ToString(), out string? serverConnectionId))
                {
                    var whatsappRequest = await _messagingService.OnSendingMessage(whatsapp);
                    await _hubContext.Clients.Client(serverConnectionId).SendAsync(SignalRKeys.SendWhatsapp, whatsappRequest);
                    ++numberOfSentMessages;
                    --numberOfPendingMessages;
                    _shareContactRequests.Add(new SharingContactRequest
                    {
                        AppId = new Guid(whatsappRequest.AppId.ToString()),
                        ServerConnectionId = serverConnectionId,
                        MobileServerId = new Guid(whatsappRequest.MobileServerId.ToString()),
                        ReceiverNumber = whatsappRequest.ReceiverNumber,
                        TransactionId = new Guid(transactionId)
                    });
                }
            }

            var shareContactRequest = new ShareContactListRequest
            {
                SharingContactRequests = _shareContactRequests
            };
            await ShareContact(shareContactRequest);

            return Created(_uriService.GetMessageByTransactionId(ApiRoutes.Whatsapp.Controller, transactionId), new SendMessageResponse
            {
                SentMessages = numberOfSentMessages,
                PendingMessages = numberOfPendingMessages,
                TransactionId = transactionId.ToString()
            });
        }

        private Task ShareContact(ShareContactListRequest shareContactRequest)
        {
            var bearerToken = HttpContext.Request.Headers[HeaderNames.Authorization].FirstOrDefault(x => x.Contains("Bearer"))?.Replace("Bearer ", "");
            if (!string.IsNullOrEmpty(bearerToken))
            {
                var client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
                var request = new HttpRequestMessage(HttpMethod.Post, $"{_uriService.InternetUrl}/{ApiRoutes.Contact.Share}");
                HttpContent httpContent = new StringContent(JsonSerializer.Serialize(shareContactRequest), Encoding.UTF8, "application/json");
                request.Content = httpContent;
                client.SendAsync(request);
            }
            return Task.CompletedTask;
        }

    }
}
