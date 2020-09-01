using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OneSms.Contracts.V1;
using OneSms.Contracts.V1.Requests;
using OneSms.Contracts.V1.Responses;
using OneSms.Domain;
using OneSms.Online.Services;

namespace OneSms.Controllers.V1
{
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class SmsController : ControllerBase
    {
        private readonly ISmsService _smsService;
        private readonly IMapper _mapper;

        public SmsController(ISmsService smsService, IMapper mapper)
        {
            _smsService = smsService;
            _mapper = mapper;
        }

        [HttpPost(ApiRoutes.Sms.Send)]
        public async Task<IActionResult> SendMessage(SendMessageRequest messageRequest)
        {
            var numberOfSentMessages = 0;
            var transactionId = Guid.NewGuid();
            await foreach (var sentSms in _smsService.Send(messageRequest,transactionId.ToString()))
                numberOfSentMessages++;
            var baseUrl = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
            return Created($"{baseUrl}{ApiRoutes.Sms.GetAllByTransactionId}", new SendMessageResponse
            {
                SentMessages = numberOfSentMessages,
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
    }
}
