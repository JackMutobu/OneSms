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
    public class SmsController : ControllerBase
    {
        private OneSmsDbContext _oneSmsDbContext;
        private HubEventService _smsHubEventService;
        private SmsDataExtractorService _smsDataExtractorService;

        public SmsController(OneSmsDbContext oneSmsDbContext,HubEventService smsHubEventService,SmsDataExtractorService smsDataExtractorService)
        {
            _oneSmsDbContext = oneSmsDbContext;
            _smsHubEventService = smsHubEventService;
            _smsDataExtractorService = smsDataExtractorService;
        }

        [HttpPut("StatusChanged")]
        public async Task<IActionResult> SmsStatusChanged([FromBody]SmsTransactionDto sms)
        {
            var smsTransaction = _oneSmsDbContext.SmsTransactions.First(x => x.Id == sms.SmsId);
            smsTransaction.CompletedTime = sms.TimeStamp;
            smsTransaction.TransactionState = sms.TransactionState;
            _oneSmsDbContext.Update(smsTransaction);
            await _oneSmsDbContext.SaveChangesAsync();
            _smsHubEventService.OnSmsStateChanged.OnNext(sms);
            Debug.WriteLine($"SmsId:{sms.SmsId}, State:{sms.TransactionState}, Time:{(smsTransaction.CompletedTime - smsTransaction.StartTime).TotalSeconds} seconds");
            return Ok("Status changed");
        }

        [HttpPut("SmsReceived")]
        public async Task<IActionResult> OnSmsReceived([FromBody]SmsReceivedDto sms)
        {
            var smsData = await _smsDataExtractorService.GetSmsData(sms);
            if (smsData.SimId != 0)
            {
                var sim = _oneSmsDbContext.Sims.First(x => x.Id == smsData.SimId);
                switch (smsData.UssdActionType)
                {
                    case UssdActionType.AirtimeBalance:
                        sim.AirtimeBalance = smsData.Balance;
                        break;
                    case UssdActionType.CallBalance:
                        sim.CallBalance = smsData.Balance;
                        break;
                    case UssdActionType.MobileMoneyBalance:
                        sim.MobileMoneyBalance = smsData.Balance;
                        break;
                    case UssdActionType.SmsBalance:
                        sim.SmsBalance = smsData.Balance;
                        break;
                }
                sim.UpdatedOn = DateTime.UtcNow;
                _oneSmsDbContext.Update(sim);
                await _oneSmsDbContext.SaveChangesAsync();
                return Ok("Sim balance changed");
            }
            return Ok("Regex not found");
        }
    }
}
