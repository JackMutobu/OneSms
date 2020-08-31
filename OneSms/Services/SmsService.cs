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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OneSms.Online.Services
{
    public interface ISmsService
    {
        IAsyncEnumerable<int> Send(SendMessageRequest sendMessageRequest, string transactionId = "");
        IAsyncEnumerable<int> SendPending(string serverKey);
        IAsyncEnumerable<SmsMessage> GetMessages(Guid appId);
    }

    public class SmsService : ISmsService
    {
        private DataContext _dbContext;
        private IServerConnectionService _serverConnectionService;
        private IHubContext<OneSmsHub> _hubContext;

        public SmsService(DataContext dbContext, IServerConnectionService serverConnectionService, IHubContext<OneSmsHub> hubContext)
        {
            _dbContext = dbContext;
            _serverConnectionService = serverConnectionService;
            _hubContext = hubContext;
        }

        public async IAsyncEnumerable<int> Send(SendMessageRequest sendMessageRequest, string transactionId = "")
        {
            await foreach (var sms in RegisterSendMessageRequest(sendMessageRequest,transactionId))
                yield return await Send(sms);
        }

        public async IAsyncEnumerable<int> SendPending(string serverKey)
        {
            var pendingMessages = await _dbContext.SmsMessages.Where(x => x.MobileServerId.ToString() == serverKey && x.MessageStatus == MessageStatus.Pending).ToListAsync();
            foreach(var sms in pendingMessages)
                yield return await Send(sms);
        }

        public  IAsyncEnumerable<SmsMessage> GetMessages(Guid appId) 
            => _dbContext.SmsMessages.Include(x => x.MobileServer).OrderByDescending(x => x.CompletedTime)
            .Where(x => x.AppId == appId).AsAsyncEnumerable();

        private async Task<int> Send(SmsMessage sms)
        {
            var smsRequest = new SmsRequest
            {
                AppId = sms.AppId,
                Body = sms.Body,
                SenderNumber = sms.SenderNumber,
                MobileServerId = sms.MobileServerId,
                ReceiverNumber = sms.RecieverNumber,
                SmsId = sms.Id,
                TransactionId = sms.TransactionId,
                SimSlot = _dbContext.Sims.Single(x => x.Number == sms.SenderNumber && x.MobileServerId == sms.MobileServerId).SimSlot
            };

            if (_serverConnectionService.ConnectedServers.TryGetValue(sms.MobileServerId.ToString(), out string? serverConnectionId))
            {
                sms.MessageStatus = MessageStatus.Sending;
                _dbContext.Update(sms);
                await _dbContext.SaveChangesAsync();
                await _hubContext.Clients.Client(serverConnectionId).SendAsync(SignalRKeys.SendSms, smsRequest);
                return 1;
            }
            return 0;
        }

        private async IAsyncEnumerable<SmsMessage> RegisterSendMessageRequest(SendMessageRequest sendMessageRequest, string transId = "")
        {
            var transactionId = string.IsNullOrEmpty(transId) ? Guid.NewGuid() : new Guid(transId);
            var mobileServerId = _dbContext.Sims.SingleOrDefault(x => x.Number == sendMessageRequest.SenderNumber)?.MobileServerId;
            if (mobileServerId != null)
            {
                foreach (var recipient in sendMessageRequest.Recipients)
                {
                    var sms = new SmsMessage
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
                        TransactionId = transactionId,
                        Tags = sendMessageRequest.Tags
                    };
                    EntityEntry<SmsMessage> created = _dbContext.SmsMessages.Add(sms);
                    await _dbContext.SaveChangesAsync();
                    sms.Id = created.Entity.Id;
                    yield return sms;
                }
            }
        }
    }
}
