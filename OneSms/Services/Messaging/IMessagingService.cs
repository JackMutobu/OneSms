using OneSms.Contracts.V1.Dtos;
using OneSms.Contracts.V1.MobileServerRequest;
using OneSms.Contracts.V1.Requests;
using OneSms.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OneSms.Services
{
    public interface IMessagingService<T,U,V> where T:BaseMessage where U:BaseMessagingRequest where V:BaseMessageReceived
    {
        IAsyncEnumerable<T> GetMessages(Guid appId);
        IAsyncEnumerable<T> GetMessagesByTransactionId(Guid transactionId);
        Task<U> OnSendingMessage(T message);
        Task<T> OnStatusChanged(U request, DateTime completedTime);
        IAsyncEnumerable<T> RegisterSendMessageRequest(SendMessageRequest sendMessageRequest, string transId = "");
        IAsyncEnumerable<T> GetPendingMessages(string serverKey);
        Task<List<T>> GetListOfPendingMessages(string serverKey);
        Task<bool> CheckSenderNumber(string number);
        string? GetSenderNumber(Guid AppId);
        Task<T?> OnMessageReceived(V messageReceived, DateTime receivedTime);
    }
}
