using OneSms.Web.Shared.Dtos;
using System.Reactive.Subjects;
using Splat;
using OneSms.Domain;
using System;

namespace OneSms.Online.Services
{
    public class HubEventService:IEnableLogger
    {
        public HubEventService()
        {
            OnUssdStateChanged = new Subject<UssdTransactionDto>();
            OnMessageStateChanged = new Subject<MessageTransactionProcessDto>();
            OnSmsMessageStatusChanged = new Subject<SmsMessage>();
            OnWhatsappMessageStatusChanged = new Subject<WhatsappMessage>();
        }

        public Subject<UssdTransactionDto> OnUssdStateChanged { get; }

        public Subject<MessageTransactionProcessDto> OnMessageStateChanged { get; }

        public Subject<SmsMessage> OnSmsMessageStatusChanged { get; }

        public Subject<WhatsappMessage> OnWhatsappMessageStatusChanged { get; }

    }
}
