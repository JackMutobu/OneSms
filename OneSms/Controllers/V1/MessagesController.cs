using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OneSms.Contracts.V1;
using OneSms.Contracts.V1.Enumerations;
using OneSms.Contracts.V1.Requests;
using OneSms.Contracts.V1.Responses;
using OneSms.Domain;
using OneSms.Online.Services;
using OneSms.Services;
using System;
using System.Collections.Generic;
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

        public MessagesController(ISmsService smsService, IWhatsappService whatsappService, IMapper mapper)
        {
            _smsService = smsService;
            _whatsappService = whatsappService;
            _mapper = mapper;
        }

        [HttpPost(ApiRoutes.Message.Send)]
        public async Task<IActionResult> SendMessage(SendMessageRequest messageRequest)
        {
            var numberOfSentMessages = 0;
            var transactionId = Guid.NewGuid();
           foreach(var processor in messageRequest.Processors)
            {
                switch(processor)
                {
                    case MessageProcessor.SMS:
                        await foreach (var sentSms in _smsService.Send(messageRequest,transactionId.ToString()))
                            numberOfSentMessages++;
                        break;
                    case MessageProcessor.Whatsapp:
                        await foreach (var sentMessage in _whatsappService.Send(messageRequest,transactionId.ToString()))
                            numberOfSentMessages++;
                        break;
                }
            }
            var baseUrl = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
            return Created($"{baseUrl}{ApiRoutes.Message.GetAllByTransactionId}", new SendMessageResponse
            {
                SentMessages = numberOfSentMessages,
                TransactionId = transactionId.ToString()
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
    }
}
