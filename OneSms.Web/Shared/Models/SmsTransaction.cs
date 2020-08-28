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

        public SmsTransaction(MessageTransactionProcessDto transaction)
        {
            Body = transaction.Message;
            StartTime = transaction.TimeStamp;
            RecieverNumber = transaction.ReceiverNumber;
            SenderNumber = transaction.SenderNumber;
            OneSmsAppId = transaction.AppId;
            MobileServerId = transaction.MobileServerId;
            TransactionId = transaction.TransactionId;
            TransactionState = transaction.TransactionState;
            MessageTransactionProcessor = MessageTransactionProcessor.SMS;
        }
        

    }
}
