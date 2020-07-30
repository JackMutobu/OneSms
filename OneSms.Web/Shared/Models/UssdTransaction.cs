using OneSms.Web.Shared.Enumerations;
using System;

namespace OneSms.Web.Shared.Models
{
    public class UssdTransaction:BaseModel
    {
        public string Title { get; set; }

        public UssdActionType ActionType { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime CompletedTime { get; set; }

        public string Amount { get; set; }

        public string Balance { get; set; }

        public int SimId { get; set; }

        public int MobileServerId { get; set; }

        public ServerMobile MobileServer{ get; set; }

    }
}
