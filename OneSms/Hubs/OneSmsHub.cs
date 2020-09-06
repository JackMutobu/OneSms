using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Net.Http.Headers;
using OneSms.Services;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OneSms.Hubs
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class OneSmsHub:Hub
    {
        private readonly IServerConnectionService _serverConnectionService;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IUriService _uriService;

        public OneSmsHub(IServerConnectionService serverConnectionService,IHttpClientFactory clientFactory, IUriService uriService)
        {
            _serverConnectionService = serverConnectionService;
            _clientFactory = clientFactory;
            _uriService = uriService;

        }

        public async override Task OnConnectedAsync()
        {
            try
            {
                var serverId = Context.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;

                if (!string.IsNullOrEmpty(serverId))
                {
                    _serverConnectionService.AddServer(serverId,Context.ConnectionId);
                    await SendPendingMessage(serverId, Context.GetHttpContext());
                }
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
               _serverConnectionService.RemoveServer(Context.ConnectionId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return base.OnDisconnectedAsync(exception);
        }

        public Task CheckClientAvailabilityCallback(Guid requestId,bool response)
        {
            _serverConnectionService.CheckClientAvailabilityCallback(requestId,response);
            return Task.CompletedTask;
        }

        private Task SendPendingMessage(string serverId,HttpContext context)
        {
            var bearerToken = context.Request.Headers[HeaderNames.Authorization].FirstOrDefault(x=> x.Contains("Bearer"))?.Replace("Bearer ","");
            if(!string.IsNullOrEmpty(bearerToken))
            {
                var client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
                var request = new HttpRequestMessage(HttpMethod.Post, $"{_uriService.InternetUrl}/api/v1/messages/send/pending/{serverId}");
                client.SendAsync(request);
            }
            return Task.CompletedTask;
        }

    }
}
