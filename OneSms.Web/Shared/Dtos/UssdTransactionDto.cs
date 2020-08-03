using OneSms.Web.Shared.Enumerations;
using System;
using System.Collections.Generic;

namespace OneSms.Web.Shared.Dtos
{
    public class UssdTransactionDto
    {
        public string Title { get; set; }

        public string UssdNumber { get; set; }

        public List<string> UssdInputs { get; set; }

        public List<string> KeyProblems { get; set; }

        public List<string> KeyWelcomes { get; set; }

        public UssdActionType ActionType { get; set; }

        public UssdTransactionState TransactionState { get; set; }

        public string Amount { get; set; }

        public string Balance { get; set; }

        public int SimId { get; set; }

        public int SimSlot { get; set; }

        public int UssdTransactionId { get; set; }

        public DateTime TimeStamp { get; set; }

        public string LastMessage { get; set; }



    }
}
