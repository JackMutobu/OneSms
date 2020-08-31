using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OneSms.Contracts.V1;
using OneSms.Contracts.V1.Enumerations;
using OneSms.Contracts.V1.Requests;
using OneSms.Online.Services;
using OneSms.Services;
using System;
using System.Threading.Tasks;

namespace OneSms.Controllers.V1
{
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class MessagesController: ControllerBase
    {
        private readonly ISmsService _smsService;
        private readonly IWhatsappService _whatsappService;

        public MessagesController(ISmsService smsService, IWhatsappService whatsappService)
        {
            _smsService = smsService;
            _whatsappService = whatsappService;
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
            return Ok(numberOfSentMessages);
        }
    }
}
