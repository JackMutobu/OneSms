using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using OneSms.Contracts.V1;
using OneSms.Contracts.V1.MobileServerRequest;
using OneSms.Contracts.V1.Requests;
using OneSms.Contracts.V1.Responses;
using OneSms.Hubs;
using OneSms.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OneSms.Controllers.V1
{
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class WhatsappController : ControllerBase
    {
        private readonly IWhatsappService _whatsappService;
        private readonly IMapper _mapper;
        private readonly IUriService _uriService;
        private readonly HubEventService _hubEventService;
        private readonly IHubContext<OneSmsHub> _hubContext;
        private readonly IServerConnectionService _serverConnectionService;

        public WhatsappController(IWhatsappService whatsappService, IMapper mapper,IUriService uriService, HubEventService hubEventService,
            IHubContext<OneSmsHub> hubContext, IServerConnectionService serverConnectionService)
        {
            _whatsappService = whatsappService;
            _mapper = mapper;
            _uriService = uriService;
            _hubEventService = hubEventService;
            _hubContext = hubContext;
            _serverConnectionService = serverConnectionService;
        }

        [HttpPost(ApiRoutes.Whatsapp.Send)]
        public async Task<IActionResult> SendMessage(SendMessageRequest messageRequest)
        {
            var isSenderValid = await _whatsappService.CheckSenderNumber(messageRequest.SenderNumber);
            if(isSenderValid)
            {
                return await SendToMobileServer(messageRequest);
            }

            return BadRequest(new SendMessageFailedResponse
            {
                Errors = new List<string> { "Sender number not found" }
            });
        }

        private async Task<IActionResult> SendToMobileServer(SendMessageRequest messageRequest)
        {
            var numberOfSentMessages = 0;
            var transactionId = Guid.NewGuid().ToString();
            var numberOfPendingMessages = 0;
            string? serverConnectionId;
            await foreach (var whatsapp in _whatsappService.RegisterSendMessageRequest(messageRequest, transactionId))
            {
                ++numberOfPendingMessages;
                if (_serverConnectionService.ConnectedServers.TryGetValue(whatsapp.MobileServerId.ToString(), out serverConnectionId))
                {
                    var whatsappRequest = await _whatsappService.OnSendingMessage(whatsapp);
                    await _hubContext.Clients.Client(serverConnectionId).SendAsync(SignalRKeys.SendWhatsapp, whatsappRequest);
                    ++numberOfSentMessages;
                    --numberOfPendingMessages;
                }
            }

            return Created(_uriService.GetMessageByTransactionId(ApiRoutes.Whatsapp.Controller, transactionId), new SendMessageResponse
            {
                SentMessages = numberOfSentMessages,
                PendingMessages = numberOfPendingMessages,
                TransactionId = transactionId.ToString()
            });
        }

        [HttpGet(ApiRoutes.Whatsapp.GetAllByTransactionId)]
        public async Task<IActionResult> GetMessagesByTransactionId(string transactionId)
        {
            var messages = new List<MessageResponse>();

            await foreach (var message in _whatsappService.GetMessagesByTransactionId(new Guid(transactionId)))
            {
                var sms = _mapper.Map<MessageResponse>(message);
                messages.Add(sms);
            }

            return Ok(messages);
        }

        [HttpGet(ApiRoutes.Whatsapp.GetAllByAppId)]
        public async Task<IActionResult> GetMessagesByAppId(string appId)
        {
            var messages = new List<MessageResponse>();

            await foreach (var message in _whatsappService.GetMessages(new Guid(appId)))
            {
                var sms = _mapper.Map<MessageResponse>(message);
                messages.Add(sms);
            }

            return Ok(messages);
        }

        [HttpPut(ApiRoutes.Whatsapp.StatusChanged)]
        public async Task<IActionResult> OnStatusChanged([FromBody]WhatsappRequest whatsappRequest)
        {
            var message = await _whatsappService.OnStatusChanged(whatsappRequest, DateTime.UtcNow);
            _hubEventService.OnWhatsappMessageStatusChanged.OnNext(message);

            return Ok($"Message status changed:{message.MessageStatus}");
        }

        [HttpPost(ApiRoutes.Whatsapp.NumberNotFound)]
        public async Task<IActionResult> OnNumberNotFound([FromBody] WhatsappRequest whatsappRequest)
        {
           
            return Ok($"NumberNotFound");
        }
    }
}
