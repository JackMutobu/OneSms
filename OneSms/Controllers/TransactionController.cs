using Microsoft.AspNetCore.Mvc;
using OneSms.Data;
using OneSms.Services;
using OneSms.Web.Shared.Dtos;
using OneSms.Web.Shared.Enumerations;
using OneSms.Web.Shared.Models;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace OneSms.Online.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly OneSmsDbContext _oneSmsDbContext;
        private readonly HubEventService _hubEventService;

        public TransactionController(OneSmsDbContext oneSmsDbContext, HubEventService hubEventService)
        {
            _oneSmsDbContext = oneSmsDbContext;
            _hubEventService = hubEventService;
        }

        [HttpPut("StatusChanged")]
        public async Task<IActionResult> MessageStatusChanged([FromBody] MessageTransactionProcessDto transactionDto)
        {
            var transaction = new MessageTransaction();
           switch(transactionDto.MessageTransactionProcessor)
            {
                case MessageTransactionProcessor.Whatsapp:
                    transaction = _oneSmsDbContext.WhatsappTransactions.FirstOrDefault(x => x.Id == transactionDto.WhatsappId);
                    break;
                case MessageTransactionProcessor.SMS:
                    transaction = _oneSmsDbContext.SmsTransactions.FirstOrDefault(x => x.Id == transactionDto.SmsId);
                    break;
            }

            transaction.CompletedTime = transactionDto.TimeStamp;
            transaction.TransactionState = transactionDto.TransactionState;
            _oneSmsDbContext.Update(transaction);
            await _oneSmsDbContext.SaveChangesAsync();

            _hubEventService.OnMessageStateChanged.OnNext(transactionDto);

            Debug.WriteLine($"Id:{transaction.Id}, State:{transaction.TransactionState}, Time:{(transaction.CompletedTime - transaction.StartTime).TotalSeconds} seconds");
            return Ok("Status changed");
        }

        
    }
}
