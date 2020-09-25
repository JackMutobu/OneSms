using OneSms.Contracts.V1.Enumerations;
using System;
using System.Collections.Generic;

namespace OneSms.Contracts.V1.MobileServerRequest
{
    public class UssdRequest
    {
        public string UssdNumber { get; set; } = null!;

        public List<string> UssdInputs { get; set; }

        public List<string> KeyProblems { get; set; }

        public List<string> KeyWelcomes { get; set; }

        public NetworkActionType NetworkAction { get; set; }

        public Guid TransactionId { get; set; }

        public int UssdId { get; set; }

        public int SimId { get; set; }

        public int SimSlot { get; set; }

        public Guid MobileServerId { get; set; }
    }
}
