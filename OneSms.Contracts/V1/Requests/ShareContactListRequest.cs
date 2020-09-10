using System.Collections.Generic;

namespace OneSms.Contracts.V1.Requests
{
    public class ShareContactListRequest
    {
        public List<SharingContactRequest> SharingContactRequests { get; set; }
    }
}
