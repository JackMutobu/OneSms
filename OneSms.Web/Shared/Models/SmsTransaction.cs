using System;

namespace OneSms.Web.Shared.Models
{
    public class SmsTransaction:BaseModel
    {
        public string Title { get; set; }

        public string Body { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime CompletedTime { get; set; }

        public string RecieverNumber { get; set; }

        public string SenderNumber { get; set; }

        public int MobileServerId { get; set; }

        public ServerMobile MobileServer{ get; set; }

        public int OneSmsAppId { get; set; }

        public OneSmsApp OneSmsApp { get; set; }
    }
}
