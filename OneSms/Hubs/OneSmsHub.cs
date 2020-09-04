using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using OneSms.Services;
using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OneSms.Hubs
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class OneSmsHub:Hub
    {
        private readonly IServerConnectionService _serverConnectionService;

        public OneSmsHub(IServerConnectionService serverConnectionService)
        {
            _serverConnectionService = serverConnectionService;
        }

        public async override Task OnConnectedAsync()
        {
            try
            {
                var serverId = Context.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(serverId))
                    AddServerId(serverId);
            }
            catch(Exception ex)
            {
                await Clients.Caller.SendAsync("OnException", ex);
            }
        } 

        public override Task OnDisconnectedAsync(Exception exception)
        {
            try
            {
                var connectionId = Context.ConnectionId;
                var serverId = _serverConnectionService.ConnectedServersReverse[connectionId];
                _serverConnectionService.ConnectedServers.Remove(serverId);
                _serverConnectionService.ConnectedServersReverse.Remove(connectionId);
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return base.OnDisconnectedAsync(exception);
        }

        private void AddServerId(string serverId)
        {
            if (_serverConnectionService.ConnectedServers.ContainsKey(serverId))
                _serverConnectionService.ConnectedServers[serverId] = Context.ConnectionId;
            else
                _serverConnectionService.ConnectedServers.Add(serverId, Context.ConnectionId);

            if (_serverConnectionService.ConnectedServersReverse.ContainsKey(Context.ConnectionId))
                _serverConnectionService.ConnectedServersReverse[Context.ConnectionId] = serverId;
            else
                _serverConnectionService.ConnectedServersReverse.Add(Context.ConnectionId, serverId);
        }


    }
}
