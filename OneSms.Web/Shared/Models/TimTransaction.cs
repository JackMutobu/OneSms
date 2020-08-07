using OneSms.Web.Shared.Enumerations;
using System;

namespace OneSms.Web.Shared.Models
{
    public class TimTransaction:BaseModel
    {
        public string Number { get; set; }

        public string Date { get; set; }

        public string Time { get; set; }

        public string Minutes { get; set; }

        public string LastMessage { get; set; }

        public int Cost { get; set; }

        public int? ClientId { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public TimClient Client { get; set; }

        public UssdTransactionState TransactionState { get; set; }
    }
}
