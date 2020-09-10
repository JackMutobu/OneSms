using OneSms.Contracts.V1.Enumerations;
using System;
using System.Collections.Generic;

namespace OneSms.Contracts.V1.MobileServerRequest
{
    public class WhatsappRequest:BaseWhatsappRequest
    {
        public Guid TransactionId { get; set; }

        public int WhatsappId { get; set; }

        public MessageStatus MessageStatus { get; set; }

        public Guid AppId { get; set; }

        public Guid MobileServerId { get; set; }

        public IEnumerable<string> ImageLinks { get; set; }
    }
}
