using OneSms.Contracts.V1.Enumerations;
using OneSms.Contracts.V1.MobileServerRequest;
using System;

namespace OneSms.Contracts.V1.Dtos
{
    public class BaseMessageReceived:BaseMessageRequest
    {
        public MessageStatus MessageStatus { get; set; }

        public DateTime ReceivedDateTime { get; set; }

        public DateTime CompleteReceivedDateTime { get; set; }

        public string MobileServerKey { get; set; }
    }
}
