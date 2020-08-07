using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using OneSms.Online.Data;
using OneSms.Online.Hubs;
using OneSms.Online.Services;
using OneSms.Web.Shared.Constants;
using OneSms.Web.Shared.Dtos;
using OneSms.Web.Shared.Enumerations;
using OneSms.Web.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OneSms.Online.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TimController : ControllerBase
    {
        private OneSmsDbContext _oneSmsDbContext;
        private IHubContext<OneSmsHub> _hubContext;
        private ServerConnectionService _serverConnectionService;
        private TimService _timService;

        public TimController(OneSmsDbContext oneSmsDbContext, IHubContext<OneSmsHub> hubContext, ServerConnectionService serverConnectionService,TimService timService)
        {
            _oneSmsDbContext = oneSmsDbContext;
            _hubContext = hubContext;
            _serverConnectionService = serverConnectionService;
            _timService = timService;
        }

        [HttpGet("activate")]
        public async Task<IActionResult> ActivateTransactions()
        {
            await ExecuteTransactions();
            return Ok("started");
        }

        [HttpGet("start")]
        public IActionResult StartTimerService()
        {
            _timService.StartTimer();
            return Ok("started");
        }

        private async Task ExecuteTransactions()
        {
            var endDateTime = DateTime.UtcNow.AddMinutes(20);
            var clients = await _oneSmsDbContext.TimClients.Where(x => x.ClientState == ClientState.Active && x.ActivationTime > DateTime.UtcNow && x.ActivationTime <= endDateTime).ToListAsync();
            var ussdAction = await _oneSmsDbContext.UssdActions.Include(x => x.Steps).FirstOrDefaultAsync(x => x.ActionType == UssdActionType.TimTransaction);
            var mobileServer = await _oneSmsDbContext.MobileServers.Include(x => x.Sims).FirstOrDefaultAsync(x => x.IsTimServer);
            var sims = mobileServer?.Sims;
            var serverConnectionId = string.Empty;
            if (_serverConnectionService.ConnectedServers.TryGetValue(mobileServer.Key.ToString(), out serverConnectionId))
            {
                foreach (var client in clients)
                {
                    if (ussdAction != null && mobileServer != null)
                    {
                        var inputs = GetInputs(client, ussdAction);
                        var selectedSim = sims.FirstOrDefault(x => int.Parse(x.AirtimeBalance ?? "0") > 0) ?? sims.FirstOrDefault();
                        var ussdTransaction = new TimTransaction()
                        {
                            CreatedOn = DateTime.UtcNow,
                            ClientId = client.Id,
                            StartTime = DateTime.UtcNow,
                            EndTime = DateTime.UtcNow,
                            Number = client.PhoneNumber,
                            TransactionState = UssdTransactionState.Sent,
                            Minutes = client.NumberOfMinutes.ToString()
                        };
                        var ussd = new UssdTransactionDto()
                        {
                            ActionType = UssdActionType.TimTransaction,
                            ClientId = client.Id,
                            IsTimTransaction = true,
                            KeyProblems = ussdAction.KeyProblems.Split(",").ToList(),
                            KeyWelcomes = ussdAction.KeyLogins.Split(",").ToList(),
                            UssdNumber = ussdAction.UssdNumber,
                            TimeStamp = DateTime.UtcNow,
                            SimId = selectedSim.Id,
                            SimSlot = selectedSim.SimSlot,
                            UssdTransactionId = (await AddTransaction(ussdTransaction)).Id,
                            TransactionState = UssdTransactionState.Sent,
                            UssdInputs = inputs
                        };
                        await _hubContext.Clients.Client(serverConnectionId).SendAsync(SignalRKeys.SendUssd, ussd);
                    }
                }
            }
            else
            {
                foreach (var client in clients)
                {
                    client.ActivationTime = DateTime.UtcNow.AddMinutes(20);
                    _oneSmsDbContext.Update(client);
                    await _oneSmsDbContext.SaveChangesAsync();
                }
            }
        }

        private async Task<TimTransaction> AddTransaction(TimTransaction transaction)
        {
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<TimTransaction> created = _oneSmsDbContext.TimTransactions.Add(transaction);
            await _oneSmsDbContext.SaveChangesAsync();
            return transaction;
        }

        private int GetMinuteIndex(int minutes)
        {
            if (minutes >= 0 && minutes < 3)
                return 5;
            if (minutes >= 3 && minutes < 8)
                return 4;
            if (minutes >= 8 && minutes < 16)
                return 2;
            return 1;
        }
        private List<string> GetInputs(TimClient client, UssdAction ussdAction)
        {
            var inputs = new List<string>();
            var placeHolderData = new Queue<string>();
            placeHolderData.Enqueue(client.PhoneNumber);
            placeHolderData.Enqueue(GetMinuteIndex(client.NumberOfMinutes).ToString());
            foreach (var step in ussdAction.Steps)
            {
                if (step.IsPlaceHolder)
                    step.Value = placeHolderData.Dequeue();
                inputs.Add(step.Value);
            }
            return inputs;
        }
    }
}
