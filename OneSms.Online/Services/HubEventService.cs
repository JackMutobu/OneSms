using OneSms.Web.Shared.Dtos;
using System.Reactive.Subjects;
using Splat;

namespace OneSms.Online.Services
{
    public class HubEventService:IEnableLogger
    {
        public HubEventService()
        {
            OnSmsStateChanged = new Subject<SmsTransactionDto>();
            OnUssdStateChanged = new Subject<UssdTransactionDto>();
            OnMessageStateChanged = new Subject<MessageTransactionProcessDto>();
        }

        public Subject<SmsTransactionDto> OnSmsStateChanged { get; }

        public Subject<UssdTransactionDto> OnUssdStateChanged { get; }

        public Subject<MessageTransactionProcessDto> OnMessageStateChanged { get; }
    }
}
