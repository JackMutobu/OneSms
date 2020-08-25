using OneSms.Web.Shared.Enumerations;
using System;
using System.ComponentModel.DataAnnotations;

namespace OneSms.Web.Shared.Models
{
    public class  MessageTransaction:BaseModel
    {
        public string Title { get; set; }

        [Required]
        public string Body { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime CompletedTime { get; set; }

        [Required]
        public string RecieverNumber { get; set; }

        public string SenderNumber { get; set; }

        public Guid TransactionId { get; set; }

        public MessageTransactionState TransactionState { get; set; }

        public MessageTransactionProcessor MessageTransactionProcessor { get; protected set; }

        public string Label { get; set; }

        public int MobileServerId { get; set; }

        public ServerMobile MobileServer { get; set; }

        public Guid OneSmsAppId { get; set; }

        public OneSmsApp OneSmsApp { get; set; }
    }
}
