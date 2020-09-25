using OneSms.Contracts.V1.Enumerations;
using System;

namespace OneSms.Domain
{
    public class UssdTransaction
    {
        public int Id { get; set; }

        public Guid TransactionId { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public int? UssdActionId { get; set; }

        public UssdAction? UssdAction { get; set; }

        public int SimId { get; set; }

        public SimCard? Sim { get; set; }

        public string? LastMessage { get; set; }

        public UssdTransactionState TransactionState { get; set; }
    }
}
