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
    public interface ISmsService
    {
        IAsyncEnumerable<SmsMessage> GetMessages(Guid appId);
        IAsyncEnumerable<SmsMessage> GetMessagesByTransactionId(Guid transactionId);
        Task<SmsRequest> OnSendingMessage(SmsMessage sms);
        Task<SmsMessage> OnStatusChanged(SmsRequest smsRequest, DateTime completedTime);
        IAsyncEnumerable<SmsMessage> RegisterSendMessageRequest(SendMessageRequest sendMessageRequest, string transId = "");
        IAsyncEnumerable<SmsMessage> SendPending(string serverKey);
        Task<bool> CheckSenderNumber(string number);
    }

    public class SmsService : ISmsService
    {
        private readonly DataContext _dbContext;
        private readonly IMapper _mapper;

        public SmsService(DataContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async IAsyncEnumerable<SmsMessage> RegisterSendMessageRequest(SendMessageRequest sendMessageRequest, string transId = "")
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

        public Task<bool> CheckSenderNumber(string number) => _dbContext.Sims.AnyAsync(x => x.Number == number);

        public IAsyncEnumerable<SmsMessage> SendPending(string serverKey)
        {
            return _dbContext.SmsMessages.Where(x => x.MobileServerId.ToString() == serverKey && x.MessageStatus == MessageStatus.Pending).AsAsyncEnumerable();
        }

        public IAsyncEnumerable<SmsMessage> GetMessagesByTransactionId(Guid transactionId)
            => _dbContext.SmsMessages.OrderByDescending(x => x.CompletedTime).Where(x => x.TransactionId == transactionId).AsAsyncEnumerable();

        public IAsyncEnumerable<SmsMessage> GetMessages(Guid appId)
            => _dbContext.SmsMessages.Include(x => x.MobileServer).OrderByDescending(x => x.CompletedTime)
            .Where(x => x.AppId == appId).AsAsyncEnumerable();

        public async Task<SmsMessage> OnStatusChanged(SmsRequest smsRequest, DateTime completedTime)
        {
            var sms = _dbContext.SmsMessages.FirstOrDefault(x => x.Id == smsRequest.SmsId);
            sms.CompletedTime = completedTime;
            sms.MessageStatus = smsRequest.MessageStatus;
            _dbContext.Update(sms);
            await _dbContext.SaveChangesAsync();
            return sms;
        }

        public async Task<SmsRequest> OnSendingMessage(SmsMessage sms)
        {
            sms.MessageStatus = MessageStatus.Sending;
            _dbContext.Update(sms);
            await _dbContext.SaveChangesAsync();
            var smsRequest = _mapper.Map<SmsRequest>(sms);
            smsRequest.SimSlot = _dbContext.Sims.Single(x => x.Number == sms.SenderNumber && x.MobileServerId == sms.MobileServerId).SimSlot;
            return smsRequest;
        }



    }
}
