using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using OneSms.Contracts.V1;
using OneSms.Data;
using OneSms.Domain;
using OneSms.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OneSms.Hubs
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ChatHub: Hub
    {
        private readonly DataContext _dbContext;
        private readonly HubEventService _hubEventService;
        private readonly Dictionary<string, string> _connectedClients;


        public ChatHub(DataContext dbContext, HubEventService hubEventService)
        {
            _dbContext = dbContext;
            _hubEventService = hubEventService;
            _connectedClients = new Dictionary<string, string>();

            _hubEventService.OnMessageReceived
                .Subscribe(async message => await OnMessageReceived(message));

            _hubEventService.OnMessageStateChanged
               .Subscribe(async message => await OnMessageStateChanged(message));
        }
        public async override Task OnConnectedAsync()
        {
            try
            {
                var appId = Context.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;

                if (!string.IsNullOrEmpty(appId))
                    _connectedClients.Add(appId!, Context.ConnectionId);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("OnException", ex);
            }
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            try
            {
                var appId = Context.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(appId) && _connectedClients.ContainsKey(appId))
                    _connectedClients.Remove(appId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return base.OnDisconnectedAsync(exception);
        }

        public async Task OnMessageReceived(BaseMessage message)
        {
            if(_connectedClients.TryGetValue(message.AppId.ToString(), out string? clientId))
                await Clients.Client(clientId).SendAsync(SignalRKeys.OnMessageReceived, message);
        }

        public async Task OnMessageStateChanged(BaseMessage message)
        {
            if (_connectedClients.TryGetValue(message.AppId.ToString(), out string? clientId))
                await Clients.Client(clientId).SendAsync(SignalRKeys.OnMessageSentStatusChanged, message);
        }
    }
}
