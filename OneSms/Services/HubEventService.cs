using System.Reactive.Subjects;
using Splat;
using OneSms.Domain;

namespace OneSms.Services
{
    public class HubEventService:IEnableLogger
    {
        public HubEventService()
        {
            OnUssdStateChanged = new Subject<UssdTransaction>();
            OnMessageStateChanged = new Subject<BaseMessage>();
            OnSmsMessageStatusChanged = new Subject<SmsMessage>();
            OnWhatsappMessageStatusChanged = new Subject<WhatsappMessage>();
            OnMessageReceived = new Subject<BaseMessage>();
    }

        public Subject<UssdTransaction> OnUssdStateChanged { get; }

        public Subject<BaseMessage> OnMessageStateChanged { get; }

        public Subject<BaseMessage> OnMessageReceived { get; }

        public Subject<SmsMessage> OnSmsMessageStatusChanged { get; }

        public Subject<WhatsappMessage> OnWhatsappMessageStatusChanged { get; }

    }
}
