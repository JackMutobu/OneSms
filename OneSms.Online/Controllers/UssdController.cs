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
    public class UssdController : ControllerBase
    {
        private OneSmsDbContext _oneSmsDbContext;
        private HubEventService _hubEventService;

        public UssdController(OneSmsDbContext oneSmsDbContext,HubEventService hubEventService)
        {
            _oneSmsDbContext = oneSmsDbContext;
            _hubEventService = hubEventService;
        }

        [HttpPut("StatusChanged")]
        public IActionResult SmsStatusChanged([FromBody] UssdTransactionDto ussd)
        {
            var transaction = _oneSmsDbContext.UssdTransactions.First(x => x.Id == ussd.UssdTransactionId);
            transaction.CompletedTime = ussd.TimeStamp;
            transaction.TransactionState = ussd.TransactionState;
            transaction.Balance = ussd.Balance;
            transaction.LastMessage = ussd.LastMessage;
            _oneSmsDbContext.Update(transaction);
            _oneSmsDbContext.SaveChangesAsync();
            _hubEventService.OnUssdStateChanged.OnNext(ussd);
            Debug.WriteLine($"UssdId:{ussd.UssdTransactionId},UssdNumber:{ussd.UssdNumber}, State:{ussd.TransactionState}, Time:{(transaction.CompletedTime - transaction.StartTime).TotalSeconds} seconds");
            return Ok("Status changed");
        }
    }
}
