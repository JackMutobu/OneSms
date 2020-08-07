using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
                    case UssdActionType.TimTransaction:
                        var number = smsData.Number.Substring(3);
                        var transaction = _oneSmsDbContext.TimTransactions.OrderByDescending(x=> x.StartTime).FirstOrDefault(x => x.Number == number);
                        if(transaction?.ClientId != null)
                        {
                            var client = _oneSmsDbContext.TimClients.FirstOrDefault(x => x.Id == transaction.ClientId);
                            client.ActivationTime = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(40)).AddDays(1);
                            _oneSmsDbContext.Update(client);
                        }
                        if(transaction != null)
                        {
                            transaction.Minutes = smsData.Minutes;
                            transaction.TransactionState = UssdTransactionState.Confirmed;
                            transaction.EndTime = DateTime.UtcNow;
                            transaction.Cost = int.Parse(smsData.Cost);
                            _oneSmsDbContext.Update(transaction);
                        }
                        sim.AirtimeBalance = (int.Parse(sim.AirtimeBalance ?? "0") - transaction.Cost).ToString();
                        break;
                }

                sim.UpdatedOn = DateTime.UtcNow;
                _oneSmsDbContext.Update(sim);
                await _oneSmsDbContext.SaveChangesAsync();
                return Ok("Sim balance changed");
            }
            return Ok("Regex not found");
        }

        [HttpPost("Status")]
        public async Task<IActionResult> StatusChanged([FromBody] SmsModel sms)
        {
            var smsRec = new SmsReceivedDto
            {
                Body = sms.MessageBody,
                OriginatingAddress = sms.OriginatingAddress,
                SimSlot = 0,
                MobileServerKey = "57042d03-b322-4745-86d2-43487e515274"
            };
            var smsData = await _smsDataExtractorService.GetSmsData(smsRec);
            
            return Ok(smsData);
        }
    }
}
