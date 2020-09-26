using OneSms.Contracts.V1.Enumerations;
using System;

namespace OneSms.Contracts.V1.Requests
{
    public class UssdApiRequest
    {
        public NetworkActionType NetworkAction { get; set; }

        public UssdTransactionState TransactionState { get; set; }

        public Guid TransactionId { get; set; }

        public int UssdId { get; set; }

        public int SimId { get; set; }

        public int SimSlot { get; set; }

        public Guid MobileServerId { get; set; }

        public string LastMessage { get; set; }
    }
}
