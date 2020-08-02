using OneSms.Web.Shared.Dtos;
using System.Reactive.Subjects;
using Splat;

namespace OneSms.Online.Services
{
    public class SmsHubEventService:IEnableLogger
    {
        public SmsHubEventService()
        {
            OnSmsStateChanged = new Subject<SmsTransactionDto>();
        }

        public Subject<SmsTransactionDto> OnSmsStateChanged { get; }
    }
}
