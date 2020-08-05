using OneSms.Web.Shared.Enumerations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;

namespace OneSms.Web.Shared.Models
{
    public class TimClient:BaseModel
    {
        public TimClient()
        {
            Transactions = new Collection<TimTransaction>();
        }
        public string Names { get; set; }

        public string PhoneNumber { get; set; }

        public int NumberOfMinutes { get; set; }

        public DateTime ActivationTime { get; set; }

        public ClientState ClientState { get; set; }

        public ICollection<TimTransaction> Transactions { get; set; }
    }
}
