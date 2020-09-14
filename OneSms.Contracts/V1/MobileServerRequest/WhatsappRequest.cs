using OneSms.Contracts.V1.Enumerations;
using System;
using System.Collections.Generic;

namespace OneSms.Contracts.V1.MobileServerRequest
{
    public class WhatsappRequest:BaseMessagingRequest
    {
        public IEnumerable<string> ImageLinks { get; set; }
    }
}
