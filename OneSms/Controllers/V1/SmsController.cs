using System;
using System.Collections.Generic;
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
using OneSms.Online.Services;
using OneSms.Services;

namespace OneSms.Controllers.V1
{
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class SmsController : ControllerBase
    {
        private readonly ISmsService _smsService;
        private readonly IMapper _mapper;
        private readonly IUriService _uriService;
        private readonly HubEventService _hubEventService;
        private readonly IHubContext<OneSmsHub> _hubContext;
        private readonly IServerConnectionService _serverConnectionService;

        public SmsController(ISmsService smsService, IMapper mapper,IUriService uriService,HubEventService hubEventService,
            IHubContext<OneSmsHub> hubContext, IServerConnectionService serverConnectionService)
        {
            _smsService = smsService;
            _mapper = mapper;
            _uriService = uriService;
            _hubEventService = hubEventService;
            _hubContext = hubContext;
            _serverConnectionService = serverConnectionService;
        }

        [HttpPost(ApiRoutes.Sms.Send)]
        public async Task<IActionResult> SendMessage(SendMessageRequest messageRequest)
        {
            var numberOfSentMessages = 0;
            var numberOfPendingMessages = 0;
            var transactionId = Guid.NewGuid().ToString();
            string? serverConnectionId;
            await foreach (var sms in _smsService.RegisterSendMessageRequest(messageRequest, transactionId))
            {
                ++numberOfPendingMessages;
                if (_serverConnectionService.ConnectedServers.TryGetValue(sms.MobileServerId.ToString(), out serverConnectionId))
                {
                    var smsRequest = await _smsService.OnSendingMessage(sms);
                    await _hubContext.Clients.Client(serverConnectionId).SendAsync(SignalRKeys.SendSms, smsRequest);
                    ++numberOfSentMessages;
                    --numberOfPendingMessages;
                }
            }

            return Created(_uriService.GetMessageByTransactionId(ApiRoutes.Sms.Controller,transactionId), new SendMessageResponse
            {
                SentMessages = numberOfSentMessages,
                PendingMessages = numberOfPendingMessages,
                TransactionId = transactionId.ToString()
            });
        }

        [HttpGet(ApiRoutes.Sms.GetAllByTransactionId)]
        public async Task<IActionResult> GetMessagesByTransactionId(string transactionId)
        {
            var messages = new List<MessageResponse>();

            await foreach (var message in _smsService.GetMessagesByTransactionId(new Guid(transactionId)))
            {
                var sms = _mapper.Map<MessageResponse>(message);
                messages.Add(sms);
            }

            return Ok(messages);
        }

        [HttpGet(ApiRoutes.Sms.GetAllByAppId)]
        public async Task<IActionResult> GetMessagesByAppId(string appId)
        {
            var messages = new List<MessageResponse>();

            await foreach (var message in _smsService.GetMessages(new Guid(appId)))
            {
                var sms = _mapper.Map<MessageResponse>(message);
                messages.Add(sms);
            }

            return Ok(messages);
        }

        [HttpPut(ApiRoutes.Sms.StatusChanged)]
        public async Task<IActionResult> OnStatusChanged([FromBody] SmsRequest smsRequest)
        {
            var message = await _smsService.OnStatusChanged(smsRequest, DateTime.UtcNow);
            _hubEventService.OnSmsMessageStatusChanged.OnNext(message);

            return Ok($"Message status changed:{message.MessageStatus}");
        }

        [HttpPut(ApiRoutes.Sms.SmsReceived)]
        public  IActionResult OnSmsReceived([FromBody]SmsReceived smsReceived)
        {
            return Ok("Received");
        }
    }
}
