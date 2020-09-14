using OneSms.Contracts.V1.Enumerations;
using System;

namespace OneSms.Contracts.V1.MobileServerRequest
{
    public class BaseMessagingRequest:BaseMessageRequest
    {
        public Guid TransactionId { get; set; }

        public MessageStatus MessageStatus { get; set; }

        public Guid AppId { get; set; }

        public Guid MobileServerId { get; set; }

        public int MessageId { get; set; }
    }
}
