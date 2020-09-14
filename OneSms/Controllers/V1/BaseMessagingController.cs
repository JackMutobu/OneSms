using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OneSms.Contracts.V1.MobileServerRequest;
using OneSms.Contracts.V1.Requests;
using OneSms.Contracts.V1.Responses;
using OneSms.Domain;
using OneSms.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OneSms.Controllers.V1
{
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public abstract class BaseMessagingController<T,U>:ControllerBase where T:BaseMessage where U: BaseMessagingRequest
    {
        protected readonly IMessagingService<T, U> _messagingService;
        protected readonly IMapper _mapper;
        private readonly HubEventService _hubEventService;

        public BaseMessagingController(IMessagingService<T,U> messagingService, IMapper mapper, HubEventService hubEventService)
        {
            _messagingService = messagingService;
            _mapper = mapper;
            _hubEventService = hubEventService;
        }

        public virtual async Task<IActionResult> SendMessage(SendMessageRequest messageRequest)
        {
            if (string.IsNullOrEmpty(messageRequest.SenderNumber))
                messageRequest.SenderNumber = _messagingService.GetSenderNumber(messageRequest.AppId);

            if (!string.IsNullOrEmpty(messageRequest.SenderNumber))
            {
                var isSenderValid = await _messagingService.CheckSenderNumber(messageRequest.SenderNumber);
                if (isSenderValid)
                    return await SendToMobileServer(messageRequest);
            }

            return BadRequest(new SendMessageFailedResponse
            {
                Errors = new List<string> { "Sender number not found" }
            });
        }

        public virtual async Task<IActionResult> GetMessagesByTransactionId(string transactionId)
        {
            var messages = new List<MessageResponse>();

            await foreach (var message in _messagingService.GetMessagesByTransactionId(new Guid(transactionId)))
            {
                var sms = _mapper.Map<MessageResponse>(message);
                messages.Add(sms);
            }

            return Ok(messages);
        }

        public virtual async Task<IActionResult> GetMessagesByAppId(string appId)
        {
            var messages = new List<MessageResponse>();

            await foreach (var message in _messagingService.GetMessages(new Guid(appId)))
            {
                var responseMessage = _mapper.Map<MessageResponse>(message);
                messages.Add(responseMessage);
            }

            return Ok(messages);
        }

        public virtual async Task<IActionResult> OnStatusChanged([FromBody]U request)
        {
            var message = await _messagingService.OnStatusChanged(request, DateTime.UtcNow);
            if (message != null)
                _hubEventService.OnMessageStateChanged.OnNext(message);

            return Ok($"Message status changed:{message?.MessageStatus}");
        }

        protected abstract Task<IActionResult> SendToMobileServer(SendMessageRequest messageRequest);

       
    }
}
