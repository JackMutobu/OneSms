using OneSms.Contracts.V1.Enumerations;
using System;

namespace OneSms.Contracts.V1.MobileServerRequest
{
    public class SmsRequest
    {
        public Guid TransactionId { get; set; }

        public string Body { get; set; }

        public int SimSlot { get; set; }

        public int SmsId { get; set; }

        public MessageStatus MessageStatus { get; set; }

        public string ReceiverNumber { get; set; }

        public string SenderNumber { get; set; }

        public Guid AppId { get; set; }

        public Guid MobileServerId { get; set; }
    }
}
