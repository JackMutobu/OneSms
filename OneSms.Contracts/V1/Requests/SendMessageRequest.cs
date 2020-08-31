using OneSms.Contracts.V1.Enumerations;
using System;
using System.Collections.Generic;

namespace OneSms.Contracts.V1.Requests
{
    public class SendMessageRequest
    {

        public string Label { get; set; }

        public string SenderNumber { get; set; }

        public IEnumerable<string> Recipients { get; set; }

        public string Body { get; set; }

        public string Tags { get; set; }

        public Guid AppId { get; set; }

        public IEnumerable<MessageProcessor> Processors { get; set; }

        public List<string> ImageLink { get; set; }
    }
}
