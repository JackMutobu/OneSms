using Microsoft.AspNetCore.Mvc;
using OneSms.Data;
using OneSms.Online.Data;
using OneSms.Online.Services;
using OneSms.Web.Shared.Dtos;
using OneSms.Web.Shared.Enumerations;
using OneSms.Web.Shared.Models;
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
            if (ussd.ActionType == UssdActionType.TimTransaction)
            {
                var transaction = _oneSmsDbContext.TimTransactions.First(x => x.Id == ussd.UssdTransactionId && (ussd.ClientId == 0 || x.ClientId == ussd.ClientId));
                transaction.TransactionState = ussd.TransactionState;
                transaction.LastMessage = ussd.LastMessage;
                transaction.EndTime = ussd.TimeStamp;
                _oneSmsDbContext.Update(transaction);
            }
            else
            {
                var transaction = _oneSmsDbContext.UssdTransactions.First(x => x.Id == ussd.UssdTransactionId);
                transaction.CompletedTime = ussd.TimeStamp;
                transaction.TransactionState = ussd.TransactionState;
                transaction.Balance = ussd.Balance;
                transaction.LastMessage = ussd.LastMessage;
                _oneSmsDbContext.Update(transaction);
                Debug.WriteLine($"UssdId:{ussd.UssdTransactionId},UssdNumber:{ussd.UssdNumber}, State:{ussd.TransactionState}, Time:{(transaction.CompletedTime - transaction.StartTime).TotalSeconds} seconds");
            }

            await _oneSmsDbContext.SaveChangesAsync();
            await _simService.CheckIfIsBalanceUpdate(ussd.LastMessage, ussd.SimId).ConfigureAwait(false);
            _hubEventService.OnUssdStateChanged.OnNext(ussd);
           
            return Ok("Status changed");
        }

        


    }
}
