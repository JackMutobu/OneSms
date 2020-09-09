using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using OneSms.Contracts.V1;
using OneSms.Contracts.V1.MobileServerRequest;
using OneSms.Contracts.V1.Requests;
using OneSms.Hubs;
using OneSms.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OneSms.Controllers.V1
{
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ContactController : ControllerBase
    {
        private readonly IContactService _contactService;
        private readonly IHubContext<OneSmsHub> _hubContext;

        public ContactController(IContactService contactService,IHubContext<OneSmsHub> hubContext)
        {
            _contactService = contactService;
            _hubContext = hubContext;
        }

        [HttpPost(ApiRoutes.Contact.Share)]
        public async Task<IActionResult> ShareContact([FromBody] SharingContactRequest request)
        {
            var isAlreadyAContact = _contactService.IsAlreadyContacted(request.AppId.ToString(), request.Number);
            if (!isAlreadyAContact)
            {
                var appContactVcard = await _contactService.AddContactToApp(request.AppId.ToString(), request.Number);

                if (!string.IsNullOrEmpty(appContactVcard))
                {
                    var contactRequest = new ShareContactRequest
                    {
                        Message = "Pour recevoir plus d'information à propos de nous, veuillez enregistrer nos contacts ",
                        VcardInfo = appContactVcard,
                        Number = request.Number
                    };
                    var messageRequest = new WhatsappRequest()
                    {
                        Body = contactRequest.Message,
                        ReceiverNumber = request.Number,
                        AppId = request.AppId,
                        MobileServerId = request.MobileServerId,
                        SenderNumber = request.Number,
                        TransactionId = request.TransactionId,
                        ImageLinks = new List<string>(),
                        WhatsappId = GetRandomNumber()
                    };
                    await _hubContext.Clients.Client(request.ServerConnectionId).SendAsync(SignalRKeys.ShareContact, contactRequest);
                    await Task.Delay(TimeSpan.FromSeconds(1));//Wait a few seconds
                    await _hubContext.Clients.Client(request.ServerConnectionId).SendAsync(SignalRKeys.SendWhatsapp, messageRequest);
                }
            }
            return Ok("Shared");
        }

        private int GetRandomNumber()
        {
            Random rnd = new Random();  // creates a number between 1 and 6
            return -rnd.Next(int.MaxValue);
        }
    }
}
