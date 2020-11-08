using OneSms.Contracts.V1.Enumerations;
using System;

namespace OneSms.Domain
{
    public class NetworkMessageData
    {
        public int Id { get; set; }

        public NetworkActionType NetworkAction { get; set; }

        public decimal Amount { get; set; }

        public decimal Cost { get; set; }

        public DateTime ExecutionDate { get; set; }

        public DateTime ExpiryDate { get; set; }

        public string Message { get; set; } = null!;

        public int SimId { get; set; }

        public SimCard? Sim { get; set; }
    }
}
