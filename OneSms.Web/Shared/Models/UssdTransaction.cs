using OneSms.Web.Shared.Dtos;
using OneSms.Web.Shared.Enumerations;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace OneSms.Web.Shared.Models
{
    public class UssdTransaction:BaseModel
    {
        public UssdTransaction()
        {

        }

        public UssdTransaction(UssdTransactionDto ussd)
        {
            Title = ussd.Title;
            SimId = ussd.SimId;
            TransactionState = ussd.TransactionState;
            ActionType = ussd.ActionType;
        }

        public string Title { get; set; }

        public UssdActionType ActionType { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime CompletedTime { get; set; }

        public string Amount { get; set; }

        public string Balance { get; set; }

        public string LastMessage { get; set; }

        public int? SimId { get; set; }

        public int MobileServerId { get; set; }

        public UssdTransactionState TransactionState { get; set; }

        public ServerMobile MobileServer{ get; set; }

        [ForeignKey(nameof(SimId))]
        public SimCard Sim { get; set; }

    }
}
