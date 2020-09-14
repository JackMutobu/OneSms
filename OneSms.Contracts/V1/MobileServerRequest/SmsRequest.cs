using OneSms.Contracts.V1.Enumerations;
using System;

namespace OneSms.Contracts.V1.MobileServerRequest
{
    public class SmsRequest:BaseMessagingRequest
    {
        public int SimSlot { get; set; }
    }
}
