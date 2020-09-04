using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using OneSms.Data;
using OneSms.Services;
using System;
using System.Diagnostics;
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
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public override Task OnConnectedAsync()
        {
            var user = Context.User.Identity;
            return base.OnConnectedAsync();
        }

        public async Task SetServerId(string serverKey)
        {
            try
            {
                if (_serverConnectionService.ConnectedServers.ContainsKey(serverKey))
                    _serverConnectionService.ConnectedServers[serverKey] = Context.ConnectionId;
                else
                    _serverConnectionService.ConnectedServers.Add(serverKey, Context.ConnectionId);

                if(_serverConnectionService.ConnectedServersReverse.ContainsKey(Context.ConnectionId))
                    _serverConnectionService.ConnectedServersReverse[Context.ConnectionId] = serverKey;
                else
                    _serverConnectionService.ConnectedServersReverse.Add(Context.ConnectionId, serverKey);

                await Clients.Caller.SendAsync("OnConnected", Context.ConnectionId);
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

    }
}
