using Microsoft.AspNetCore.SignalR;
using OneSms.Online.Data;
using OneSms.Online.Hubs;
using OneSms.Web.Shared.Models;
using System.Linq;

namespace OneSms.Online.Services
{
    public class SmsService
    {
        private IHubContext<OneSmsHub> _oneSmsHubContext;
        private ServerConnectionService _serverConnectionService;
        private OneSmsDbContext _oneSmsDbContext;

        public SmsService(IHubContext<OneSmsHub> oneSmsHubContext, ServerConnectionService serverConnectionService,OneSmsDbContext oneSmsDbContext)
        {
            _oneSmsHubContext = oneSmsHubContext;
            _serverConnectionService = serverConnectionService;
            _oneSmsDbContext = oneSmsDbContext;
        }
        

    }
}
