using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace OneSms.Domain
{
    public class SimCard
    {
        public SimCard()
        {
            Apps = new Collection<ApplicationSim>();
        }
        public int Id { get; set; }

        [Required]
        [DataType(DataType.PhoneNumber)]
        public string Number { get; set; } = null!;

        [Required]
        public string Name { get; set; } = null!;

        public string? SmsBalance { get; set; }

        public decimal MinSmsBalance { get; set; }

        public string? MobileMoneyBalance { get; set; }

        public string? AirtimeBalance { get; set; }

        public decimal MinAirtimeBalance { get; set; }

        public string? CallBalance { get; set; }

        public int SimSlot { get; set; }

        public bool IsWhatsappNumber { get; set; }

        public DateTime UpdatedOn { get; set; }

        public int NetworkId { get; set; }

        public virtual NetworkOperator? Network { get; set; }

        public Guid MobileServerId { get; set; }

        public virtual MobileServer? MobileServer { get; set; }

        public ICollection<ApplicationSim> Apps { get; set; }
    }
}
