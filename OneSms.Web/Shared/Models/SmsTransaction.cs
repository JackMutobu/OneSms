using OneSms.Web.Shared.Dtos;
using OneSms.Web.Shared.Enumerations;

namespace OneSms.Web.Shared.Models
{
    public class SmsTransaction:MessageTransaction
    {
        public SmsTransaction() 
        {
            MessageTransactionProcessor = MessageTransactionProcessor.SMS;
        }

        public SmsTransaction(SmsTransactionDto smsTransactionDto)
        {
            Body = smsTransactionDto.Message;
            StartTime = smsTransactionDto.TimeStamp;
            RecieverNumber = smsTransactionDto.ReceiverNumber;
            SenderNumber = smsTransactionDto.SenderNumber;
            OneSmsAppId = smsTransactionDto.AppId;
            MobileServerId = smsTransactionDto.MobileServerId;
            Title = smsTransactionDto.Title;

            MessageTransactionProcessor = MessageTransactionProcessor.SMS;
        }
        

    }
}
