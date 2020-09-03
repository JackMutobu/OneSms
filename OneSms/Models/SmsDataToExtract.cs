using OneSms.Web.Shared.Enumerations;

namespace OneSms.Models
{
    public class SmsDataToExtract
    {
        public string Number { get; set; }

        public string Balance { get; set; }

        public string Cost { get; set; }

        public string Minutes { get; set; }

        public int SimId { get; set; }

        public int MobileServerId { get; set; }

        public UssdActionType UssdActionType { get; set; }
    }
}
