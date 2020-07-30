using System;

namespace OneSms.Web.Shared.Models
{
    public class AppSim
    {
        public Guid AppId { get; set; }
        public OneSmsApp App { get; set; }

        public int SimId { get; set; }
        public SimCard Sim { get; set; }
    }
}
