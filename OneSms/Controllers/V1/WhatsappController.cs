using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OneSms.Contracts.V1;
using OneSms.Contracts.V1.Requests;
using OneSms.Services;
using System.Threading.Tasks;

namespace OneSms.Controllers.V1
{
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class WhatsappController : ControllerBase
    {
        private readonly IWhatsappService _whatsappService;

        public WhatsappController(IWhatsappService whatsappService)
        {
            _whatsappService = whatsappService;
        }

        [HttpPost(ApiRoutes.Whatsapp.Send)]
        public async Task<IActionResult> SendMessage(SendMessageRequest messageRequest)
        {
            var numberOfSentMessages = 0;
            await foreach (var sentMessage in _whatsappService.Send(messageRequest))
                numberOfSentMessages++;
            return Ok(numberOfSentMessages);
        }
    }
}
