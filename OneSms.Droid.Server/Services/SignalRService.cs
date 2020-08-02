using Android.Content;
using Android.Widget;
using Microsoft.AspNetCore.SignalR.Client;
using OneSms.Web.Shared.Dtos;
using System;
using System.Threading.Tasks;

namespace OneSms.Droid.Server.Services
{
    public class SignalRService
    {
        public SignalRService(Context context,string url)
        {
            _context = context;
            Connection = new HubConnectionBuilder()
               .WithUrl(url)
               .Build();
            Connection.Closed += async (error) =>
            {
                IsConnected = false;
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await ConnectToHub();
            };
            Connection.On<string>("OnConnected", conId =>
            {
                var id = conId;
            });
            Connection.On<Exception>("OnException", ex => Toast.MakeText(_context, ex.Message, ToastLength.Long).Show());
        }

        private Context _context;

        public HubConnection Connection { get; }

        public bool IsConnected { get; private set; }

        public async Task<bool> ConnectToHub()
        {
            try
            {
                await Connection.StartAsync();
                IsConnected = true;
            }
            catch (Exception ex)
            {
                IsConnected = false;
            }
            return IsConnected;
        }


        public async Task SendServerId(string serverId)
        {
            if (Connection.State == HubConnectionState.Connected)
                await Connection.InvokeAsync("SetServerId", serverId);
        }

        public async Task SendSmsStateChanged(SmsTransactionDto smsTransactionDto)
        {
            if(Connection.State == HubConnectionState.Connected)
                await Connection.InvokeAsync("SmsStateChanged", smsTransactionDto);
        }

        

    }
}