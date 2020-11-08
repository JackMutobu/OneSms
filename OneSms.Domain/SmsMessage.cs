namespace OneSms.Domain
{
    public class SmsMessage:BaseMessage
    {
        public SmsMessage()
        {
            MessageProcessor = Contracts.V1.Enumerations.MessageProcessor.SMS;
        }
    }
}
