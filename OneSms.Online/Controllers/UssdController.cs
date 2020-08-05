using Microsoft.AspNetCore.Mvc;
using OneSms.Online.Data;
using OneSms.Online.Services;
using OneSms.Web.Shared.Dtos;
using OneSms.Web.Shared.Enumerations;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace OneSms.Online.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UssdController : ControllerBase
    {
        private OneSmsDbContext _oneSmsDbContext;
        private HubEventService _hubEventService;
        private SimService _simService;

        public UssdController(OneSmsDbContext oneSmsDbContext,HubEventService hubEventService,SimService simService)
        {
            _oneSmsDbContext = oneSmsDbContext;
            _hubEventService = hubEventService;
            _simService = simService;
        }

        [HttpPut("StatusChanged")]
        public async Task<IActionResult> SmsStatusChanged([FromBody] UssdTransactionDto ussd)
        {
            var transaction = _oneSmsDbContext.UssdTransactions.First(x => x.Id == ussd.UssdTransactionId);
            transaction.CompletedTime = ussd.TimeStamp;
            transaction.TransactionState = ussd.TransactionState;
            transaction.Balance = ussd.Balance;
            transaction.LastMessage = ussd.LastMessage;
            _oneSmsDbContext.Update(transaction);
            await _oneSmsDbContext.SaveChangesAsync();
            _hubEventService.OnUssdStateChanged.OnNext(ussd);
            await _simService.CheckIfIsBalanceUpdate(ussd.LastMessage, ussd.SimId).ConfigureAwait(false);
            Debug.WriteLine($"UssdId:{ussd.UssdTransactionId},UssdNumber:{ussd.UssdNumber}, State:{ussd.TransactionState}, Time:{(transaction.CompletedTime - transaction.StartTime).TotalSeconds} seconds");
            return Ok("Status changed");
        }

        
    }
}
