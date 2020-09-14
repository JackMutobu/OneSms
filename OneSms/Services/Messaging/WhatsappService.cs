using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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
    public interface IWhatsappService: IMessagingService<WhatsappMessage, WhatsappRequest> { }

    public class WhatsappService :BaseMessagingService<WhatsappMessage, WhatsappRequest>, IWhatsappService
    {
        private readonly DataContext _dbContext;

        public WhatsappService(DataContext dbContext, IMapper mapper):base(dbContext,mapper)
        {
            _dbContext = dbContext;
        }

        public override async IAsyncEnumerable<WhatsappMessage> RegisterSendMessageRequest(SendMessageRequest sendMessageRequest, string transactionId = "")
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
                        MessageStatus = MessageStatus.Pending,
                        MobileServerId = (Guid)mobileServerId,
                        RecieverNumber = recipient,
                        SenderNumber = sendMessageRequest.SenderNumber,
                        TransactionId = transId,
                        Tags = sendMessageRequest.Tags,
                        ImageLinkOne = sendMessageRequest.ImageLink.Where(x => !string.IsNullOrEmpty(x)).FirstOrDefault(),
                        ImageLinkTwo = sendMessageRequest.ImageLink.Where(x => !string.IsNullOrEmpty(x)).Skip(1).FirstOrDefault(),
                        ImageLinkThree = sendMessageRequest.ImageLink.Where(x => !string.IsNullOrEmpty(x)).Skip(2).FirstOrDefault()
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
