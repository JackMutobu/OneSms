namespace OneSms.Web.Shared.Models
{
    public class SmsMessage
    {

        public string OriginatingAddress { get; set; }

        public string ServiceCenterNumber { get; set; }

        public string Body { get; set; }
    }
}
