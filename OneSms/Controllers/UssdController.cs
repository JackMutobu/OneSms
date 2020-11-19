using Microsoft.AspNetCore.Mvc;
using OneSms.Data;
using OneSms.Domain;
using OneSms.Services;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace OneSms.Online.Controllers
{
   c
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
        public async Task<IActionResult> SmsStatusChanged([FromBody] UssdTransaction ussd)
        {
            if (ussd.UssdAction == UssdActionType.TimTransaction)
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
