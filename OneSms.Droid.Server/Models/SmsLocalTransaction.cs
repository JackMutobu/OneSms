using OneSms.Droid.Server.Enumerations;
using System;

namespace OneSms.Droid.Server.Models
{
    public class SmsLocalTransaction
    {
        public SmsLocalTransaction(string sender, string receiver, int smsId, string timeStamp,TransactionState transactionState)
        {
            SenderPhone = sender;
            ReceiverPhone = receiver;
            SmsId = smsId;
            StartTimeStamp = timeStamp;
            TransactionState = transactionState;
        }
        public string SenderPhone { get; set; }

        public string ReceiverPhone { get; set; }

        public int SendeId { get; set; }

        public string StartTimeStamp { get; set; }

        public int SmsId { get; set; }

        public TransactionState TransactionState { get; set; }

        public DateTime TransactionTime { get; set; }
    }
}