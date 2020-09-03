using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using OneSms.Data;
using OneSms.Hubs;
using OneSms.Services;
using OneSms.Web.Shared.Constants;
using OneSms.Web.Shared.Dtos;
using OneSms.Web.Shared.Enumerations;
using OneSms.Web.Shared.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OneSms.Online.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SmsController : ControllerBase
    {
        private OneSmsDbContext _oneSmsDbContext;
        private SmsDataExtractorService _smsDataExtractorService;
        private IHubContext<OneSmsHub> _oneSmsHub;
        private ServerConnectionService _serverConnectionService;

        public SmsController(OneSmsDbContext oneSmsDbContext,SmsDataExtractorService smsDataExtractorService, IHubContext<OneSmsHub> oneSmsHub, ServerConnectionService serverConnectionService)
        {
            _oneSmsDbContext = oneSmsDbContext;
            _smsDataExtractorService = smsDataExtractorService;
            _oneSmsHub = oneSmsHub;
            _serverConnectionService = serverConnectionService;
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

        [HttpPost("Send")]
        public async Task<IActionResult> SendSms([FromBody] MessageTransactionProcessDto messageTransaction)
        {
            var smsTransaction = new SmsTransaction(messageTransaction)
            {
                Title = "SMS sent from controller",
                TransactionState = MessageTransactionState.Sending
            };
            var mobileServer = _oneSmsDbContext.MobileServers.Include(x => x.Sims).FirstOrDefault(x => x.Id == messageTransaction.MobileServerId);
            var simCard = mobileServer?.Sims?.FirstOrDefault();
            smsTransaction.SenderNumber = simCard?.Number;
            messageTransaction.SenderNumber = simCard?.Number;
            messageTransaction.SimSlot = simCard?.SimSlot ?? 1;
            messageTransaction.TransactionState = MessageTransactionState.Sending;

            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<SmsTransaction> created = _oneSmsDbContext.SmsTransactions.Add(smsTransaction);
            await _oneSmsDbContext.SaveChangesAsync();
            messageTransaction.SmsId = created.Entity.Id;
            var serverConnectionId = string.Empty;
            if (_serverConnectionService.ConnectedServers.TryGetValue(mobileServer.Key.ToString(), out serverConnectionId))
                await _oneSmsHub.Clients.Client(serverConnectionId).SendAsync(SignalRKeys.SendSms, messageTransaction);
            else
                return Ok("Mobile server not conencted");
            return Ok("sms sent");
        }
    }
}
