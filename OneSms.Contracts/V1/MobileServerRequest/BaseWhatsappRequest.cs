namespace OneSms.Contracts.V1.MobileServerRequest
{
    public class BaseWhatsappRequest
    {
        public string Body { get; set; }

        public string ReceiverNumber { get; set; }

        public string SenderNumber { get; set; }
    }
}
