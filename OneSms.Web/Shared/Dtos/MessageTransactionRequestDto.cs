using OneSms.Web.Shared.Enumerations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OneSms.Web.Shared.Dtos
{
    public class MessageTransactionRequestDto
    {
        public MessageTransactionRequestDto()
        {
            TransactionProcessors = new List<MessageTransactionProcessor>();
        }

        [Required]
        public string Title { get; set; }

        public string SenderNumber { get; set; }

        [Required]
        public List<string> Recipients { get; set; }

        public string Message { get; set; }

        public string Label { get; set; }

        public Guid AppId { get; set; }

        public List<MessageTransactionProcessor> TransactionProcessors { get; set; }

        public List<string> ImageLinks { get; set; }

    }
}
