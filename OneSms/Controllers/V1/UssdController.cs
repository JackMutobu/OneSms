using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using OneSms.Contracts.V1;
using OneSms.Contracts.V1.Requests;
using OneSms.Data;
using OneSms.Hubs;
using OneSms.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OneSms.Controllers.V1
{
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UssdController: ControllerBase
    {
        private readonly DataContext _dbContext;
        private readonly HubEventService _hubEventService;
        private readonly INetworkMessageExtractionService _messageExtractionService;
        private readonly ISimCardManagementService _simCardManagementService;
        private readonly IHubContext<OneSmsHub> _hubContext;
        private readonly IServerConnectionService _serverConnectionService;

        public UssdController(DataContext dbContext,HubEventService hubEventService, INetworkMessageExtractionService messageExtractionService, 
            ISimCardManagementService simCardManagementService, IHubContext<OneSmsHub> hubContext, IServerConnectionService serverConnectionService)
        {
            _dbContext = dbContext;
            _hubEventService = hubEventService;
            _messageExtractionService = messageExtractionService;
            _simCardManagementService = simCardManagementService;
            _hubContext = hubContext;
            _serverConnectionService = serverConnectionService;
        }
        [HttpPut(ApiRoutes.Ussd.StatusChanged)]
        public async Task<IActionResult> OnStatusChanged([FromBody] UssdApiRequest request)
        {
            var ussdTransaction = _dbContext.UssdTransactions.FirstOrDefault(x => x.Id == request.UssdId);
            if(ussdTransaction != null)
            {
                ussdTransaction.TransactionState = request.TransactionState;
                ussdTransaction.LastMessage = request.LastMessage;
                ussdTransaction.EndTime = DateTime.UtcNow;
                _dbContext.Update(ussdTransaction);
                await _dbContext.SaveChangesAsync();
                _hubEventService.OnUssdStateChanged.OnNext(ussdTransaction);

                var messageData = _messageExtractionService.GetMessageData(request);
                if (messageData != null)
                {
                    var savedInstances = await _messageExtractionService.SaveNetworkMessageData(messageData);
                    var ussdRequest = await _simCardManagementService.ProcessNetworkMessage(messageData);
                    if (ussdRequest != null)
                    {
                        if (_serverConnectionService.ConnectedServers.TryGetValue(ussdRequest.MobileServerId.ToString(), out string? serverConnectionId))
                        {
                            await _hubContext.Clients.Client(serverConnectionId).SendAsync(SignalRKeys.SendUssd, ussdRequest);
                        }
                    }
                    return Ok($"Ussd message processed:{savedInstances}");
                }

                return Ok("Ussd transaction updated");
            }
            return NotFound("Transaction not found");
        }
    }
}
