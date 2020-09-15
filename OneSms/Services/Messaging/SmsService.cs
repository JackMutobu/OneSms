using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using OneSms.Contracts.V1.Dtos;
using OneSms.Contracts.V1.Enumerations;
using OneSms.Contracts.V1.MobileServerRequest;
using OneSms.Contracts.V1.Requests;
using OneSms.Data;
using OneSms.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OneSms.Services
{
    public interface ISmsService : IMessagingService<SmsMessage, SmsRequest,SmsReceived> { }

    public class SmsService : BaseMessagingService<SmsMessage, SmsRequest,SmsReceived>,ISmsService
    {
        private readonly DataContext _dbContext;

        public SmsService(DataContext dbContext, IMapper mapper):base(dbContext,mapper)
        {
            _dbContext = dbContext;
        }

        public override async IAsyncEnumerable<SmsMessage> RegisterSendMessageRequest(SendMessageRequest sendMessageRequest, string transId = "")
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
                        MessageStatus = MessageStatus.Pending,
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

        protected override SimCard GetReceiver(SmsReceived messageReceived)
        {
            return _dbContext.Sims
                .Include(x => x.Apps)
                .FirstOrDefault(x => x.MobileServerId.ToString() == messageReceived.MobileServerKey && x.SimSlot == messageReceived.SimSlot);
        }

        protected async override Task<SmsMessage> SaveReceivedMessage(SmsReceived messageReceived, DateTime receivedTime, SimCard receiver, ApplicationSim appSim)
        {
            var message = new SmsMessage
            {
                AppId = appSim.AppId,
                Body = messageReceived.Body,
                CompletedTime = receivedTime,
                StartTime = receivedTime,
                MessageStatus = MessageStatus.Received,
                MobileServerId = receiver.MobileServerId,
                SenderNumber = messageReceived.SenderNumber,
                RecieverNumber = receiver.Number,
                TransactionId = Guid.NewGuid(),
                Label = "Message received"
            };

            EntityEntry<SmsMessage> created = _dbContext.SmsMessages.Add(message);
            await _dbContext.SaveChangesAsync();
            message.Id = created.Entity.Id;
            return message;
        }
    }
}
