namespace OneSms.Contracts.V1.Dtos
{
    public class BaseMessageReceived
    {
        public string SenderNumber { get; set; }

        public string Body { get; set; }

        public string MobileServerKey { get; set; }

    }
}
