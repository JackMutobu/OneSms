using System;

namespace OneSms.Contracts.V1.Requests
{
    public class SharingContactRequest
    {
        public Guid AppId { get; set; }

        public string Number { get; set; }

        public string ServerConnectionId { get; set; }

        public Guid MobileServerId { get; set; }

        public Guid TransactionId { get; set; }
    }
}
