using AutoMapper;
using Microsoft.EntityFrameworkCore;
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
    public abstract class BaseMessagingService<T, U, V> : IMessagingService<T, U,V> where T : BaseMessage where U : BaseMessagingRequest where V:BaseMessageReceived
    {
        protected DataContext _dbContext;
        protected IMapper _mapper;

        public BaseMessagingService(DataContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public Task<bool> CheckSenderNumber(string number) 
            => _dbContext.Sims.AnyAsync(x => x.Number == number);

        public IAsyncEnumerable<T> GetMessages(Guid appId)
         => _dbContext.Set<T>()
            .OrderByDescending(x => x.CompletedTime)
            .Include(x => x.MobileServer)
            .Where(x => x.AppId == appId)
            .AsAsyncEnumerable();

        public IAsyncEnumerable<T> GetMessagesByTransactionId(Guid transactionId)
            => _dbContext.Set<T>()
            .OrderByDescending(x => x.CompletedTime)
            .Where(x => x.TransactionId == transactionId)
            .AsAsyncEnumerable();

        public IAsyncEnumerable<T> GetPendingMessages(string serverKey)
            => _dbContext.Set<T>()
            .Where(x => x.MobileServerId.ToString() == serverKey && x.MessageStatus == MessageStatus.Pending)
            .AsAsyncEnumerable();

        public virtual async Task<U> OnSendingMessage(T message)
        {
            message.MessageStatus = MessageStatus.Sending;
            _dbContext.Update(message);
            await _dbContext.SaveChangesAsync();
            return _mapper.Map<U>(message);
        }

        public async Task<T> OnStatusChanged(U request, DateTime completedTime)
        {
            var message = _dbContext.Set<T>().First(x => x.Id == request.MessageId);
            message.CompletedTime = completedTime;
            message.MessageStatus = request.MessageStatus;
            _dbContext.Update(message);
            await _dbContext.SaveChangesAsync();
            return message;
        }

        public string? GetSenderNumber(Guid AppId)
           => _dbContext.AppSims.Include(x => x.Sim).FirstOrDefault(x => x.AppId == AppId)?.Sim?.Number;

        public abstract IAsyncEnumerable<T> RegisterSendMessageRequest(SendMessageRequest sendMessageRequest, string transId = "");

        public async Task<T?> OnMessageReceived(V messageReceived, DateTime receivedTime)
        {
            SimCard receiver = GetReceiver(messageReceived);

            if (receiver != null)
            {
                var appSim = receiver.Apps.FirstOrDefault();
                T message = await SaveReceivedMessage(messageReceived, receivedTime, receiver, appSim);

                return message;
            }
            return null;
        }

        protected abstract Task<T> SaveReceivedMessage(V messageReceived, DateTime receivedTime, SimCard receiver, ApplicationSim appSim);

        protected abstract SimCard GetReceiver(V messageReceived);
    }
}
