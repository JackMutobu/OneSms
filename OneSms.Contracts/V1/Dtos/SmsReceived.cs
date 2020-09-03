namespace OneSms.Contracts.V1.Dtos
{
    public class SmsReceived
    {
        public string OriginatingAddress { get; set; }

        public int SimSlot { get; set; }

        public string Body { get; set; }

        public string MobileServerKey { get; set; }
    }
}
