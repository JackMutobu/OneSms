using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace OneSms.Web.Shared.Models
{
    public class SimCard:BaseModel
    {
        public SimCard()
        {
            UssdTransactions = new Collection<UssdTransaction>();
            Apps = new Collection<AppSim>();
        }
        [Required]
        [DataType(DataType.PhoneNumber)]
        public string Number { get; set; }

        [Required]
        public string Name { get; set; }

        public int NetworkId { get; set; } 

        public string SmsBalance { get; set; }

        public string MobileMoneyBalance { get; set; }

        public string AirtimeBalance { get; set; }

        public NetworkOperator Network { get; set; }

        public ICollection<UssdTransaction> UssdTransactions { get; set; }

        public ICollection<AppSim> Apps { get; set; }

        public int SimSlot { get; set; }

        public int MobileServerId { get; set; }

        public ServerMobile MobileServer { get; set; }

    }
}
