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
    public interface IWhatsappService
    {
        IAsyncEnumerable<WhatsappMessage> GetMessages(Guid appId);
        IAsyncEnumerable<WhatsappMessage> GetMessagesByTransactionId(Guid transactionId);
        Task<WhatsappRequest> OnSendingMessage(WhatsappMessage message);
        Task<WhatsappMessage> OnStatusChanged(WhatsappRequest whatsappRequest, DateTime completedTime);
        IAsyncEnumerable<WhatsappMessage> RegisterSendMessageRequest(SendMessageRequest sendMessageRequest, string transactionId = "");
        IAsyncEnumerable<WhatsappMessage> GetPendingMessages(string serverKey);
        Task<List<WhatsappMessage>> GetListOfPendingMessages(string serverKey);
        Task<bool> CheckSenderNumber(string number);
    }

    public class WhatsappService : IWhatsappService
    {
        private readonly DataContext _dbContext;
        private readonly IMapper _mapper;

        public WhatsappService(DataContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async IAsyncEnumerable<WhatsappMessage> RegisterSendMessageRequest(SendMessageRequest sendMessageRequest, string transactionId = "")
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

        public Task<bool> CheckSenderNumber(string number) => _dbContext.Sims.AnyAsync(x => x.Number == number);

        public IAsyncEnumerable<WhatsappMessage> GetPendingMessages(string serverKey)
        {
            return _dbContext.WhatsappMessages.Where(x => x.MobileServerId.ToString() == serverKey && x.MessageStatus == MessageStatus.Pending).AsAsyncEnumerable();
        }

        public  Task<List<WhatsappMessage>> GetListOfPendingMessages(string serverKey)
        {
            return _dbContext.WhatsappMessages.Where(x => x.MobileServerId.ToString() == serverKey && x.MessageStatus == MessageStatus.Pending).ToListAsync();
        }

        public IAsyncEnumerable<WhatsappMessage> GetMessagesByTransactionId(Guid transactionId)
           => _dbContext.WhatsappMessages.OrderByDescending(x => x.CompletedTime).Where(x => x.TransactionId == transactionId).AsAsyncEnumerable();

        public IAsyncEnumerable<WhatsappMessage> GetMessages(Guid appId)
            => _dbContext.WhatsappMessages.Include(x => x.MobileServer).OrderByDescending(x => x.CompletedTime)
            .Where(x => x.AppId == appId).AsAsyncEnumerable();

        public async Task<WhatsappMessage> OnStatusChanged(WhatsappRequest whatsappRequest, DateTime completedTime)
        {
            var message = _dbContext.WhatsappMessages.FirstOrDefault(x => x.Id == whatsappRequest.WhatsappId);
            message.CompletedTime = completedTime;
            message.MessageStatus = whatsappRequest.MessageStatus;
            _dbContext.Update(message);
            await _dbContext.SaveChangesAsync();
            return message;
        }

        public async Task<WhatsappRequest> OnSendingMessage(WhatsappMessage message)
        {
            message.MessageStatus = MessageStatus.Sending;
            _dbContext.Update(message);
            await _dbContext.SaveChangesAsync();
            return _mapper.Map<WhatsappRequest>(message);
        }
    }
}
