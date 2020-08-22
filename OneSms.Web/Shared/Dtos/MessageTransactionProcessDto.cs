using OneSms.Web.Shared.Enumerations;
using System;
using System.Collections.Generic;

namespace OneSms.Web.Shared.Dtos
{
    public class MessageTransactionProcessDto
    {
        public DateTime TimeStamp { get; set; }

        public string Message { get; set; }

        public int SimSlot { get; set; }

        public int SmsId { get; set; }

        public int WhatsappId { get; set; }

        public string ReceiverNumber { get; set; }

        public Guid AppId { get; set; }

        public int MobileServerId { get; set; }

        public MessageTransactionState TransactionState { get; set; }

        public MessageTransactionProcessor MessageTransactionProcessor { get; protected set; }

        public List<string> ImageLinks { get; set; }
    }
}
