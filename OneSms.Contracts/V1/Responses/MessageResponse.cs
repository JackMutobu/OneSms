using OneSms.Contracts.V1.Enumerations;
using System;
using System.Collections.Generic;

namespace OneSms.Contracts.V1.Responses
{
    public class MessageResponse
    {
        public int Id { get; set; }

        public string Body { get; set; } = null!;

        public DateTime StartTime { get; set; }

        public DateTime CompletedTime { get; set; }

        public string RecieverNumber { get; set; } = null!;

        public string? SenderNumber { get; set; }

        public Guid TransactionId { get; set; }

        public MessageStatus MessageStatus { get; set; }

        public MessageProcessor MessageProcessor { get; protected set; }

        public string? Tags { get; set; }

        public string Label { get; set; } = null!;

        public Guid AppId { get; set; }

        public List<string> Images { get; set; }
    }
}
