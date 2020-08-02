using OneSms.Web.Shared.Dtos;
using OneSms.Web.Shared.Enumerations;
using System;
using System.ComponentModel.DataAnnotations;

namespace OneSms.Web.Shared.Models
{
    public class SmsTransaction:BaseModel
    {
        public SmsTransaction() { }

        public SmsTransaction(SmsTransactionDto smsTransactionDto)
        {
            Body = smsTransactionDto.Message;
            StartTime = smsTransactionDto.TimeStamp;
            RecieverNumber = smsTransactionDto.ReceiverNumber;
            SenderNumber = smsTransactionDto.SenderNumber;
            OneSmsAppId = smsTransactionDto.AppId;
            MobileServerId = smsTransactionDto.MobileServerId;
            Title = smsTransactionDto.Title;
        }
        public string Title { get; set; }

        [Required]
        public string Body { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime CompletedTime { get; set; }

        [Required]
        public string RecieverNumber { get; set; }

        [Required]
        public string SenderNumber { get; set; }

        public int MobileServerId { get; set; }

        public SmsTransactionState TransactionState { get; set; }

        public ServerMobile MobileServer{ get; set; }

        public Guid OneSmsAppId { get; set; }

        public OneSmsApp OneSmsApp { get; set; }
    }
}
