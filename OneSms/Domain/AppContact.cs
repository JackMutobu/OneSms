using System;

namespace OneSms.Domain
{
    public class AppContact
    {
        public Guid AppId { get; set; }
        public Application? App { get; set; }

        public int ContactId { get; set; }
        public Contact? Contact { get; set; }
    }
}
