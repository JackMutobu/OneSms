using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using OneSms.Contracts.V1;
using OneSms.Contracts.V1.Enumerations;
using OneSms.Contracts.V1.MobileServerRequest;
using OneSms.Contracts.V1.Requests;
using OneSms.Data;
using OneSms.Domain;
using OneSms.Online.Hubs;
using OneSms.Online.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OneSms.Services
{
    public interface IWhatsappService
    {
        IAsyncEnumerable<int> Send(SendMessageRequest sendMessageRequest, string transactionId = "");
        IAsyncEnumerable<int> SendPending(string serverKey);
        IAsyncEnumerable<WhatsappMessage> GetMessages(Guid appId);
    }

    public class WhatsappService : IWhatsappService
    {
        private readonly DataContext _dbContext;
        private readonly IServerConnectionService _serverConnectionService;
        private readonly IHubContext<OneSmsHub> _hubContext;

        public WhatsappService(DataContext dbContext, IServerConnectionService serverConnectionService, IHubContext<OneSmsHub> hubContext)
        {
            _dbContext = dbContext;
            _serverConnectionService = serverConnectionService;
            _hubContext = hubContext;
        }

        public async IAsyncEnumerable<int> Send(SendMessageRequest sendMessageRequest, string transactionId = "")
        {
            await foreach (var message in RegisterSendMessageRequest(sendMessageRequest,transactionId))
                yield return await Send(message);
        }

        public async IAsyncEnumerable<int> SendPending(string serverKey)
        {
            var pendingMessages = await _dbContext.WhatsappMessages.Where(x => x.MobileServerId.ToString() == serverKey && x.MessageStatus == MessageStatus.Pending).ToListAsync();
            foreach (var message in pendingMessages)
                yield return await Send(message);
        }

        public IAsyncEnumerable<WhatsappMessage> GetMessages(Guid appId)
            => _dbContext.WhatsappMessages.Include(x => x.MobileServer).OrderByDescending(x => x.CompletedTime)
            .Where(x => x.AppId == appId).AsAsyncEnumerable();

        private async Task<int> Send(WhatsappMessage message)
        {
            var smsRequest = new WhatsappRequest
            {
                AppId = message.AppId,
                Body = message.Body,
                SenderNumber = message.SenderNumber,
                MobileServerId = message.MobileServerId,
                ReceiverNumber = message.RecieverNumber,
                WhatsappId = message.Id,
                TransactionId = message.TransactionId
            };

            if (_serverConnectionService.ConnectedServers.TryGetValue(message.MobileServerId.ToString(), out string? serverConnectionId))
            {
                message.MessageStatus = MessageStatus.Sending;
                _dbContext.Update(message);
                await _dbContext.SaveChangesAsync();
                await _hubContext.Clients.Client(serverConnectionId).SendAsync(SignalRKeys.SendWhatsapp, smsRequest);
                return 1;
            }
            return 0;
        }

        private async IAsyncEnumerable<WhatsappMessage> RegisterSendMessageRequest(SendMessageRequest sendMessageRequest, string transactionId = "")
        {
            var transId = string.IsNullOrEmpty(transactionId) ? Guid.NewGuid() : new Guid(transactionId);
            var mobileServerId = _dbContext.Sims.SingleOrDefault(x => x.Number == sendMessageRequest.SenderNumber)?.MobileServerId;
            if (mobileServerId != null)
            {
                foreach (var recipient in sendMessageRequest.Recipients)
                {
                    var message = new WhatsappMessage
                    {
                        AppId = sendMessageRequest.AppId,
                        Body = sendMessageRequest.Body,
                        StartTime = DateTime.UtcNow,
                        CompletedTime = DateTime.UtcNow,
                        Label = sendMessageRequest.Label,
                        MessageStatus = Contracts.V1.Enumerations.MessageStatus.Pending,
                        MobileServerId = (Guid)mobileServerId,
                        RecieverNumber = recipient,
                        SenderNumber = sendMessageRequest.SenderNumber,
                        TransactionId = transId,
                        Tags = sendMessageRequest.Tags
                    };
                    EntityEntry<WhatsappMessage> created = _dbContext.WhatsappMessages.Add(message);
                    await _dbContext.SaveChangesAsync();
                    message.Id = created.Entity.Id;
                    yield return message;
                }
            }
        }
    }
}
