using OneSms.Web.Shared.Enumerations;
using OneSms.Web.Shared.Models;

namespace OneSms.Models
{
    public class SmsDataExtractor:BaseModel
    {
        public string Description { get; set; }

        public string RegexPatern { get; set; }

        public string OriginatingAddress { get; set; }

        public int NetworkId { get; set; }

        public NetworkOperator Network { get; set; }

        public UssdActionType UssdAction { get; set; }
    }
}
