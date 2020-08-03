using Microsoft.AspNetCore.Mvc;
using OneSms.Online.Data;
using OneSms.Online.Services;
using OneSms.Web.Shared.Dtos;
using System.Diagnostics;
using System.Linq;

namespace OneSms.Online.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SmsController : ControllerBase
    {
        private OneSmsDbContext _oneSmsDbContext;
        private HubEventService _smsHubEventService;

        public SmsController(OneSmsDbContext oneSmsDbContext,HubEventService smsHubEventService)
        {
            _oneSmsDbContext = oneSmsDbContext;
            _smsHubEventService = smsHubEventService;
        }

        [HttpPut("StatusChanged")]
        public IActionResult SmsStatusChanged([FromBody]SmsTransactionDto sms)
        {
            var smsTransaction = _oneSmsDbContext.SmsTransactions.First(x => x.Id == sms.SmsId);
            smsTransaction.CompletedTime = sms.TimeStamp;
            smsTransaction.TransactionState = sms.TransactionState;
            _oneSmsDbContext.Update(smsTransaction);
            _oneSmsDbContext.SaveChangesAsync();
            _smsHubEventService.OnSmsStateChanged.OnNext(sms);
            Debug.WriteLine($"SmsId:{sms.SmsId}, State:{sms.TransactionState}, Time:{(smsTransaction.CompletedTime - smsTransaction.StartTime).TotalSeconds} seconds");
            return Ok("Status changed");
        }
    }
}
