using Akavache;
using Android.App;
using Android.Content;
using Android.Widget;
using Microsoft.AspNetCore.SignalR.Client;
using OneSms.Contracts.V1;
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
        Context Context { get; set; }

        void ChangeUrl(string url);

        void SignalRServiceConnectionChecker();
    }

    public class SignalRService : ISignalRService
    {
        private readonly string _url;
        private readonly IAuthService _authService;

        public SignalRService(Context context, string url)
        {
            Context = context;
            OnConnectionChanged = new Subject<bool>();
            _url = Preferences.Get(OneSmsAction.ServerUrl, url);
            _authService = Locator.Current.GetService<IAuthService>();

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
            Connection.On<Exception>("OnException", ex => Toast.MakeText(Context, ex.Message, ToastLength.Long).Show());

            Connection.On<string, Guid>(SignalRKeys.CheckClientAlive, (hubMethodCallback, requestId)
                => Connection.InvokeAsync(hubMethodCallback, requestId, true));

            Connectivity.ConnectivityChanged += async (s, e) => await ReconnectToHub();

            SignalRServiceConnectionChecker();
        }

        public void SignalRServiceConnectionChecker()
        {
            Observable.Interval(TimeSpan.FromMinutes(3)).Subscribe(async number =>
            {
                if (Connection.State == HubConnectionState.Disconnected)
                    await ReconnectToHub();
            });
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
                AlertDialog alertDialog = new AlertDialog.Builder(Context).Create();
                alertDialog.SetTitle("Exception");
                alertDialog.SetMessage($"Message:{ex.Message}");
                alertDialog.Show();
            }
            
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
                var current = Connectivity.NetworkAccess;
                var isAuthenticated = await _authService.IsAuthenticated();
                if (current == NetworkAccess.Internet && isAuthenticated)
                    await Connection.StartAsync();
            }
            catch (Exception ex)
            {
                AlertDialog alertDialog = new AlertDialog.Builder(Context).Create();
                alertDialog.SetTitle("Exception");
                alertDialog.SetMessage($"Message:{ex.Message}");
                alertDialog.Show();
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
                        MainThread.BeginInvokeOnMainThread(() => Toast.MakeText(Context, "Reconnecting failed", ToastLength.Long).Show());

                    OnConnectionChanged.OnNext(IsConnected);
                }
            }
            catch(Exception ex)
            {
                AlertDialog alertDialog = new AlertDialog.Builder(Context).Create();
                alertDialog.SetTitle("Exception");
                alertDialog.SetMessage($"Message:{ex.Message}");
                alertDialog.Show();
            }
        }
    }
}