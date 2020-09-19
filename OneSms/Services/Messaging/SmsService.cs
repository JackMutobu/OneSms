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
        public SmsService(DataContext dbContext, IMapper mapper):base(dbContext,mapper)
        {
            _dbContext = dbContext;
        }

        public override async IAsyncEnumerable<SmsMessage> RegisterSendMessageRequest(SendMessageRequest sendMessageRequest, string transId = "")
        {
            var transactionId = string.IsNullOrEmpty(transId) ? Guid.NewGuid() : new Guid(transId);
            var simCards = _dbContext.Sims.Where(x => x.Number == sendMessageRequest.SenderNumber).ToList();
            if(simCards.Count() > 0)
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
                        MobileServerId = GetSimCard(recipient, simCards)?.MobileServerId ?? simCards.FirstOrDefault().MobileServerId,
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

        public async override Task<SmsRequest> OnSendingMessage(SmsMessage message)
        {
            message.MessageStatus = MessageStatus.Sending;
            var simCard = GetSimCard(message.RecieverNumber, _dbContext.Sims.Where(x => x.MobileServerId == message.MobileServerId).ToList());
            _dbContext.Update(message);
            await _dbContext.SaveChangesAsync();
            var smsRequest = _mapper.Map<SmsRequest>(message);
            smsRequest.SimSlot = simCard?.SimSlot ?? 0;
            return smsRequest;
        }

        private SimCard? GetSimCard(string recipient,List<SimCard> sims)
        {
            var receiver = recipient.Replace("+","").Substring(3,4);
            var networks = _dbContext.Networks;
            foreach(var network in networks)
            {
                var aliases = network.Alias.Split(",");

                if (network.Name == "Airtel RDC")
                    receiver = recipient.Replace("+", "").Substring(3, 1);

                foreach (var alias in aliases)
                {
                    if (receiver.Contains(alias))
                    {
                        return sims.FirstOrDefault(x => x.NetworkId == network.Id);
                    }    
                }
            }
            return null;
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
