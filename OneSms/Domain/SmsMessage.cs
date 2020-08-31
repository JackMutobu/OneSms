namespace OneSms.Domain
{
    public class SmsMessage:Message
    {
        public SmsMessage()
        {
            MessageProcessor = Contracts.V1.Enumerations.MessageProcessor.SMS;
        }
    }
}
