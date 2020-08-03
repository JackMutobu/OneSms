﻿using Android.Content;
using Android.Widget;
using Javax.Security.Auth;
using Microsoft.AspNetCore.SignalR.Client;
using OneSms.Droid.Server.Constants;
using OneSms.Web.Shared.Dtos;
using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace OneSms.Droid.Server.Services
{
    public class SignalRService
    {
        private string _url;
        public SignalRService(Context context,string url)
        {
            _context = context;
            OnConnectionChanged = new Subject<bool>();
            _url = Preferences.Get(OneSmsAction.ServerUrl, url);
            Connection = new HubConnectionBuilder()
               .WithUrl(_url)
               .Build();
            Connection.Reconnected += id => SendServerId(Preferences.Get(OneSmsAction.ServerKey,string.Empty));
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

            Observable.Interval(TimeSpan.FromMinutes(5)).Subscribe(async number =>
                {
                    if (Connection.State == HubConnectionState.Disconnected)
                        await ReconnectToHub();
                });

            Connectivity.ConnectivityChanged += async (s, e) => await ReconnectToHub();
        }

        private Context _context;

        public HubConnection Connection { get; private set; }

        public bool IsConnected { get; private set; }

        public Subject<bool> OnConnectionChanged { get; }

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
            OnConnectionChanged.OnNext(IsConnected);
            return IsConnected;
        }

        public async Task ReconnectToHub()
        {
            if (Connection.State == HubConnectionState.Connected)
                await Connection.StopAsync();
            else
            {
                var isConnected = await ConnectToHub();
                if (isConnected)
                    await SendServerId(Preferences.Get(OneSmsAction.ServerKey, string.Empty));
                else
                    MainThread.BeginInvokeOnMainThread(() => Toast.MakeText(_context, "Reconnecting failed", ToastLength.Long).Show());
            }

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

        public async Task SendUssdStateChanged(UssdTransactionDto ussdTransactionDto)
        {
            if (Connection.State == HubConnectionState.Connected)
                await Connection.InvokeAsync("SmsStateChanged", ussdTransactionDto);
        }



    }
}