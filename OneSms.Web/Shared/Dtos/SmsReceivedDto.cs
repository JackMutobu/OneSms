namespace OneSms.Web.Shared.Dtos
{
    public class SmsReceivedDto
    {
        public string OriginatingAddress { get; set; }

        public int SimSlot { get; set; }

        public string Body { get; set; }

        public string MobileServerKey { get; set; }
    }
}
