using Akavache;
using Android.App;
using Android.Content;
using Android.Widget;
using Microsoft.AspNetCore.SignalR.Client;
using OneSms.Contracts.V1;
using OneSms.Droid.Server.Constants;
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

        void ChangeUrl(string url);
    }

    public class SignalRService : ISignalRService
    {
        private readonly string _url;
        private readonly IAuthService _authService;
        private readonly Subject<bool> _pingUpdate;
        private readonly Context _context;

        public SignalRService(Context context, string url)
        {
            _context = context;
            OnConnectionChanged = new Subject<bool>();
            _url = Preferences.Get(OneSmsAction.ServerUrl, url);
            _authService = Locator.Current.GetService<IAuthService>();
            _pingUpdate = new Subject<bool>();

            BuildConnection(_url);
            Connection.Closed += async (error) =>
            {
                IsConnected = false;
                OnConnectionChanged.OnNext(IsConnected);
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await ReconnectToHub();
            };

            Connection.On<string>("OnConnected", conId =>
            {
                var id = conId;
            });
            Connection.On<Exception>("OnException", ex => Toast.MakeText(_context, ex.Message, ToastLength.Long).Show());

            Connection.On<string, Guid>(SignalRKeys.CheckClientAlive, (hubMethodCallback, requestId)
                => Connection.InvokeAsync(hubMethodCallback, requestId, true));

            Connectivity.ConnectivityChanged += async (s, e) => await ReconnectToHub();



            Observable.Interval(TimeSpan.FromSeconds(5))
                .Subscribe(async _ => 
                {
                    try
                    {
                        if (Connection.State == HubConnectionState.Connected)
                            await Connection.InvokeAsync(SignalRKeys.Ping);
                    }
                    catch(Exception ex)
                    {
                        MainThread
                        .BeginInvokeOnMainThread(() => Toast.MakeText(_context, ex.Message, ToastLength.Long).Show());
                        
                    }
                 });

            var resUpdated = _pingUpdate
                .Select(x => Observable.Interval(TimeSpan.FromSeconds(20)))
                .Switch();
            resUpdated.Subscribe(async value => await ReconnectToHub());

            _pingUpdate.OnNext(true);//Start timer;
            Connection.On<bool>(SignalRKeys.Ping, result => 
            _pingUpdate.OnNext(result)/*reset timer*/);
        }

        private void BuildConnection(string url)
        {
            try
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
            catch(Exception ex)
            {
                AlertDialog alertDialog = new AlertDialog.Builder(_context).Create();
                alertDialog.SetTitle("Exception");
                alertDialog.SetMessage($"Message:{ex.Message}");
                alertDialog.Show();
            }
            
        }

        public HubConnection Connection { get; private set; }

        public bool IsConnected { get; private set; }

        public Subject<bool> OnConnectionChanged { get; }

        public void ChangeUrl(string url) => BuildConnection(url);

        public async Task<bool> ConnectToHub()
        {
            try
            {
                var current = Connectivity.NetworkAccess;
                var isAuthenticated = await _authService.IsAuthenticated();
                if (current == NetworkAccess.Internet && isAuthenticated)
                    await Connection.StartAsync();
            }
            catch (Exception ex)
            {
                MainThread
                        .BeginInvokeOnMainThread(() => Toast.MakeText(_context, ex.Message, ToastLength.Long).Show());
            }

            return Connection.State == HubConnectionState.Connected;
        }

        public async Task ReconnectToHub()
        {
            try
            {
                if (Connection.State == HubConnectionState.Connected)
                    await Connection.StopAsync();
                else
                {
                    IsConnected = await ConnectToHub();
                    if (!IsConnected)
                        MainThread.BeginInvokeOnMainThread(() => Toast.MakeText(_context, "Reconnecting failed", ToastLength.Long).Show());

                    OnConnectionChanged.OnNext(IsConnected);
                }
            }
            catch(Exception ex)
            {
                MainThread
                       .BeginInvokeOnMainThread(() => Toast.MakeText(_context, ex.Message, ToastLength.Long).Show());
            }
        }

    }
}