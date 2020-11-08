using OneSms.Contracts.V1.Enumerations;
using System;
using System.ComponentModel.DataAnnotations;

namespace OneSms.Domain
{
    public class BaseMessage
    {
        public int Id { get; set; }

        [Required]
        public string Body { get; set; } = null!;

        public DateTime StartTime { get; set; }

        public DateTime CompletedTime { get; set; }

        [Required]
        public string RecieverNumber { get; set; } = null!;

        public string? SenderNumber { get; set; }

        public Guid TransactionId { get; set; }

        public MessageStatus MessageStatus { get; set; }

        public MessageProcessor MessageProcessor { get; protected set; }

        public string? Tags { get; set; }

        [Required]
        public string Label { get; set; } = null!;

        public Guid MobileServerId { get; set; }

        public MobileServer? MobileServer { get; set; }

        public Guid AppId { get; set; }

        public Application? App { get; set; }
    }
}
