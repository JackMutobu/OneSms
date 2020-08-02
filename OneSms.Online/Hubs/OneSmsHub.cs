﻿using Microsoft.AspNetCore.SignalR;
using OneSms.Online.Data;
using OneSms.Online.Services;
using OneSms.Web.Shared.Dtos;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace OneSms.Online.Hubs
{
    public class OneSmsHub:Hub
    {
        private readonly OneSmsDbContext _oneSmsDbContext;
        private readonly ServerConnectionService _serverConnectionService;
        private readonly SmsHubEventService _smsHubEventService;

        public OneSmsHub(OneSmsDbContext oneSmsDbContext,ServerConnectionService serverConnectionService, SmsHubEventService smsHubEventService)
        {
            _oneSmsDbContext = oneSmsDbContext;
            _serverConnectionService = serverConnectionService;
            _smsHubEventService = smsHubEventService;
        }
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public async Task SetServerId(string serverKey)
        {
            _serverConnectionService.ConnectedServers.Add(serverKey, Context.ConnectionId);
            _serverConnectionService.ConnectedServersReverse.Add(Context.ConnectionId, serverKey);
            await Clients.Caller.SendAsync("OnConnected", Context.ConnectionId);
        }

        public void SmsStateChanged(SmsTransactionDto sms)
        {
            try
            {
                var smsTransaction = _oneSmsDbContext.SmsTransactions.First(x => x.Id == sms.SmsId);
                smsTransaction.CompletedTime = sms.TimeStamp;
                smsTransaction.TransactionState = sms.TransactionState;
                _oneSmsDbContext.Update(smsTransaction);
                _oneSmsDbContext.SaveChangesAsync();
                _smsHubEventService.OnSmsStateChanged.OnNext(sms);
            }
            catch(Exception ex)
            {
                Clients.Caller.SendAsync("OnException", ex);
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