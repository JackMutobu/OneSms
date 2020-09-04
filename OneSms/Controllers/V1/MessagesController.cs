using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using OneSms.Contracts.V1;
using OneSms.Contracts.V1.Enumerations;
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
    public class MessagesController: ControllerBase
    {
        private readonly ISmsService _smsService;
        private readonly IWhatsappService _whatsappService;
        private readonly IMapper _mapper;
        private readonly IUriService _uriService;
        private readonly IHubContext<OneSmsHub> _hubContext;
        private readonly IServerConnectionService _serverConnectionService;

        public MessagesController(ISmsService smsService, IWhatsappService whatsappService, 
            IMapper mapper,IUriService uriService,IHubContext<OneSmsHub> hubContext, IServerConnectionService serverConnectionService)
        {
            _smsService = smsService;
            _whatsappService = whatsappService;
            _mapper = mapper;
            _uriService = uriService;
            _hubContext = hubContext;
            _serverConnectionService = serverConnectionService;
        }

        [HttpPost(ApiRoutes.Message.Send)]
        public async Task<IActionResult> SendMessage(SendMessageRequest messageRequest)
        {
            var numberOfSentMessages = 0;
            var numberOfPendingMessages = 0;
            var transactionId = Guid.NewGuid().ToString();
            string? serverConnectionId;
           foreach (var processor in messageRequest.Processors)
            {
                switch(processor)
                {
                    case MessageProcessor.SMS:
                        await foreach(var sms in _smsService.RegisterSendMessageRequest(messageRequest, transactionId))
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
                        break;
                    case MessageProcessor.Whatsapp:
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
                        break;
                }
            }

           

           return Created(_uriService.GetMessageByTransactionId(ApiRoutes.Message.Controller,transactionId), new SendMessageResponse
            {
                SentMessages = numberOfSentMessages,
                PendingMessages = numberOfPendingMessages,
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

        [HttpGet(ApiRoutes.Message.GetAllByAppId)]
        public async Task<IActionResult> GetMessagesByAppId(string appId)
        {
            var messages = new List<MessageResponse>();

            await foreach (var message in _smsService.GetMessages(new Guid(appId)))
            {
                var sms = _mapper.Map<MessageResponse>(message);
                messages.Add(sms);
            }

            await foreach (var message in _whatsappService.GetMessages(new Guid(appId)))
            {
                var whatsapp = _mapper.Map<MessageResponse>(message);
                messages.Add(whatsapp);
            }

            return Ok(messages);
        }


    }
}
