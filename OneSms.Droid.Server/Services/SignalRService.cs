using Akavache;
using Android.App;
using Android.Content;
using Android.Widget;
using Microsoft.AspNetCore.SignalR.Client;
using OneSms.Contracts.V1.Requests;
using OneSms.Droid.Server.Constants;
using OneSms.Web.Shared.Dtos;
using Splat;
using System;
using System.Net.Http;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace OneSms.Droid.Server.Services
{
    public interface ISignalRService
    {
        HubConnection Connection { get; }
        bool IsConnected { get; }
        Subject<bool> OnConnectionChanged { get; }

        Task<bool> ConnectToHub();
        Task ReconnectToHub();
        Task SendServerId(string serverId);
        Task SendSmsStateChanged(MessageTransactionProcessDto smsTransactionDto);
        Task SendUssdStateChanged(UssdTransactionDto ussdTransactionDto);
        Context Context { get; set; }

        void ChangeUrl(string url);
    }

    public class SignalRService : ISignalRService
    {
        private readonly string _url;

        public SignalRService(Context context, string url)
        {
            Context = context;
            OnConnectionChanged = new Subject<bool>();
            _url = Preferences.Get(OneSmsAction.ServerUrl, url);
            BuildConnection(_url);
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
            Connection.On<Exception>("OnException", ex => Toast.MakeText(Context, ex.Message, ToastLength.Long).Show());

            Observable.Interval(TimeSpan.FromMinutes(3)).Subscribe(async number =>
            {
                if (Connection.State == HubConnectionState.Disconnected)
                    await ReconnectToHub();
            });

            Connectivity.ConnectivityChanged += async (s, e) => await ReconnectToHub();
        }

        private void BuildConnection(string url)
        {
            Connection = new HubConnectionBuilder()
               .WithUrl(url, (opts) =>
               {
                   opts.AccessTokenProvider = () => BlobCache.LocalMachine.GetObject<string>(OneSmsAction.AuthKey)
                   .Catch(Observable.Return(string.Empty)).ToTask();
                   opts.HttpMessageHandlerFactory = (message) =>
                   {
                       if (message is HttpClientHandler clientHandler)
                           // bypass SSL certificate
                           clientHandler.ServerCertificateCustomValidationCallback +=
                               (sender, certificate, chain, sslPolicyErrors) => { return true; };
                       return message;
                   };
               })
               .Build();
        }

        public HubConnection Connection { get; private set; }

        public bool IsConnected { get; private set; }

        public Subject<bool> OnConnectionChanged { get; }
        public Context Context { get; set; }

        public void ChangeUrl(string url) => BuildConnection(url);


        public async Task<bool> ConnectToHub()
        {
            try
            {
                await Connection.StartAsync();
                IsConnected = true;
            }
            catch (Exception ex)
            {
                AlertDialog alertDialog = new AlertDialog.Builder(Context).Create();
                alertDialog.SetTitle("Exception");
                alertDialog.SetMessage($"Message:{ex.Message}");
                alertDialog.Show();

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
                    MainThread.BeginInvokeOnMainThread(() => Toast.MakeText(Context, "Reconnecting failed", ToastLength.Long).Show());
            }

        }


        public async Task SendServerId(string serverId)
        {
            try
            {
                if (Connection.State == HubConnectionState.Connected)
                    await Connection.InvokeAsync("SetServerId", serverId);
            }
            catch(Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(() => Toast.MakeText(Context, ex.Message, ToastLength.Long).Show());
            }
        }

        public async Task SendSmsStateChanged(MessageTransactionProcessDto smsTransactionDto)
        {
            if (Connection.State == HubConnectionState.Connected)
                await Connection.InvokeAsync("SmsStateChanged", smsTransactionDto);
        }

        public async Task SendUssdStateChanged(UssdTransactionDto ussdTransactionDto)
        {
            if (Connection.State == HubConnectionState.Connected)
                await Connection.InvokeAsync("SmsStateChanged", ussdTransactionDto);
        }



    }
}