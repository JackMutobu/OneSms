using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OneSms.Contracts.V1;
using OneSms.Contracts.V1.Requests;
using OneSms.Contracts.V1.Responses;
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

        public WhatsappController(IWhatsappService whatsappService, IMapper mapper)
        {
            _whatsappService = whatsappService;
            _mapper = mapper;
        }

        [HttpPost(ApiRoutes.Whatsapp.Send)]
        public async Task<IActionResult> SendMessage(SendMessageRequest messageRequest)
        {
            var numberOfSentMessages = 0;
            var transactionId = Guid.NewGuid();
            await foreach (var sentMessage in _whatsappService.Send(messageRequest,transactionId.ToString()))
                numberOfSentMessages++;
            var baseUrl = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
            return Created($"{baseUrl}{ApiRoutes.Message.GetAllByTransactionId}", new SendMessageResponse
            {
                SentMessages = numberOfSentMessages,
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
    }
}
