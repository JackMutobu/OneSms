using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using OneSms.Contracts.V1;
using OneSms.Contracts.V1.Dtos;
using OneSms.Contracts.V1.MobileServerRequest;
using OneSms.Contracts.V1.Requests;
using OneSms.Contracts.V1.Responses;
using OneSms.Domain;
using OneSms.Hubs;
using OneSms.Services;

namespace OneSms.Controllers.V1
{
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class SmsController : BaseMessagingController<SmsMessage,SmsRequest,SmsReceived>
    {
        private readonly IUriService _uriService;
        private readonly IHubContext<OneSmsHub> _hubContext;
        private readonly IServerConnectionService _serverConnectionService;

        public SmsController(ISmsService smsService, IMapper mapper,IUriService uriService,HubEventService hubEventService,
            IHubContext<OneSmsHub> hubContext, IServerConnectionService serverConnectionService):base(smsService,mapper,hubEventService)
        {
            _uriService = uriService;
            _hubContext = hubContext;
            _serverConnectionService = serverConnectionService;
        }

        [HttpPost(ApiRoutes.Sms.Send)]
        public override Task<IActionResult> SendMessage(SendMessageRequest messageRequest)
        {

            return base.SendMessage(messageRequest);
        }

        [HttpGet(ApiRoutes.Sms.GetAllByTransactionId)]
        public override Task<IActionResult> GetMessagesByTransactionId(string transactionId)
            => base.GetMessagesByTransactionId(transactionId);

        [HttpGet(ApiRoutes.Sms.GetAllByAppId)]
        public override Task<IActionResult> GetMessagesByAppId(string appId)
            => base.GetMessagesByTransactionId(appId);

        [HttpPut(ApiRoutes.Sms.StatusChanged)]
        public override Task<IActionResult> OnStatusChanged([FromBody] SmsRequest smsRequest)
            => base.OnStatusChanged(smsRequest);

        [HttpPut(ApiRoutes.Sms.SmsReceived)]
        public override Task<IActionResult> OnMessageReceived([FromBody] SmsReceived receivedMessage)
            => base.OnMessageReceived(receivedMessage);

        protected override async Task<IActionResult> SendToMobileServer(SendMessageRequest messageRequest)
        {
            var numberOfSentMessages = 0;
            var numberOfPendingMessages = 0;
            var transactionId = Guid.NewGuid().ToString();
            await foreach (var sms in _messagingService.RegisterSendMessageRequest(messageRequest, transactionId))
            {
                ++numberOfPendingMessages;
                if (_serverConnectionService.ConnectedServers.TryGetValue(sms.MobileServerId.ToString(), out string? serverConnectionId))
                {
                    var smsRequest = await _messagingService.OnSendingMessage(sms);
                    await _hubContext.Clients.Client(serverConnectionId).SendAsync(SignalRKeys.SendSms, smsRequest);
                    ++numberOfSentMessages;
                    --numberOfPendingMessages;
                }
            }

            return Created(_uriService.GetMessageByTransactionId(ApiRoutes.Sms.Controller, transactionId), new SendMessageResponse
            {
                SentMessages = numberOfSentMessages,
                PendingMessages = numberOfPendingMessages,
                TransactionId = transactionId.ToString()
            });
        }
    }
}
