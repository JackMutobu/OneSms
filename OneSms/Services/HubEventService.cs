using OneSms.Web.Shared.Dtos;
using System.Reactive.Subjects;
using Splat;
using OneSms.Domain;

namespace OneSms.Services
{
    public class HubEventService:IEnableLogger
    {
        public HubEventService()
        {
            OnUssdStateChanged = new Subject<UssdTransactionDto>();
            OnMessageStateChanged = new Subject<BaseMessage>();
            OnSmsMessageStatusChanged = new Subject<SmsMessage>();
            OnWhatsappMessageStatusChanged = new Subject<WhatsappMessage>();
        }

        public Subject<UssdTransactionDto> OnUssdStateChanged { get; }

        public Subject<BaseMessage> OnMessageStateChanged { get; }

        public Subject<SmsMessage> OnSmsMessageStatusChanged { get; }

        public Subject<WhatsappMessage> OnWhatsappMessageStatusChanged { get; }

    }
}
