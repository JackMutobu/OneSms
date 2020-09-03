using OneSms.Contracts.V1.Enumerations;
using System;
using System.Collections.Generic;

namespace OneSms.Contracts.V1.MobileServerRequest
{
    public class WhatsappRequest
    {
        public Guid TransactionId { get; set; }

        public string Body { get; set; }

        public int WhatsappId { get; set; }

        public string ReceiverNumber { get; set; }

        public string SenderNumber { get; set; }

        public MessageStatus MessageStatus { get; set; }

        public Guid AppId { get; set; }

        public Guid MobileServerId { get; set; }

        public IEnumerable<string> ImageLinks { get; set; }
    }
}
