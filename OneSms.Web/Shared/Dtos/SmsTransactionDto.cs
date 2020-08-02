using OneSms.Web.Shared.Enumerations;
using System;
using System.ComponentModel.DataAnnotations;

namespace OneSms.Web.Shared.Dtos
{
    public class SmsTransactionDto
    {
        public DateTime TimeStamp { get; set; }

        [Required]
        public string Title { get; set; }
        [Required]
        public string SenderNumber { get; set; }

        public int NumberOfSmsToSend { get; set; }

        [Required]
        public string ReceiverNumber { get; set; }

        [Required]
        public string Message { get; set; }

        public int SimSlot { get; set; }

        public int SmsId { get; set; }

        public Guid AppId { get; set; }

        public int MobileServerId { get; set; }

        public SmsTransactionState TransactionState { get; set; }
    }
}
