namespace OneSms.Contracts.V1.MobileServerRequest
{
    public class BaseMessageRequest
    {
        public string Body { get; set; }

        public string ReceiverNumber { get; set; }

        public string SenderNumber { get; set; }
    }
}
