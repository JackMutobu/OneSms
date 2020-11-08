using System;

namespace OneSms.Domain
{
    public class ApplicationSim
    {
        public Guid AppId { get; set; }
        public Application? App { get; set; }

        public int SimId { get; set; }
        public SimCard? Sim { get; set; }
    }
}
