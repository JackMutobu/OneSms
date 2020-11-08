using Android.Content;
using Android.Widget;
using OneSms.Droid.Server.Constants;
using OneSms.Droid.Server.Services;
using System.Threading.Tasks;
using Xamarin.Essentials;
using System.Reactive.Linq;
using System;
using Splat;
using Akavache;

namespace OneSms.Droid.Server.Views
{
    public class ServerView:LinearLayout
    {
        private ISignalRService _signalRService;
        private IHttpClientService _httpClientService;
        private IAuthService _authService;
        private TextView _label;
        private TextView _labelServer;
        private TextView _labelServerUrl;
        private EditText _serverUrlText;
        private Button _serverUrlBtn;
        private TextView _labelServerId;
        private EditText _serverText;
        private EditText _serverSecretText;
        private Button _save;
        private TextView _labelConnected;
        private Button _reconnectToSignalR;
        private Context _context;
        private Button _authBtn;
        private TextView _authStateLabel;
        public ServerView(Context context) : base(context)
        {
            _context = context;
            _signalRService = Locator.Current.GetService<ISignalRService>();
            _httpClientService = Locator.Current.GetService<IHttpClientService>();
            _authService = Locator.Current.GetService<IAuthService>();

            _label = new TextView(context)
            {
                Text = "Server ID"
            };
            _labelConnected = new TextView(context)
            {
                Text = "Disconnected",
                TextSize = 18
            };
            _serverText = new EditText(context)
            {
                Text = "54d8675f-0f08-4222-b225-08d880cb88e9",
                Hint = "Server ID..."
            };
            _serverSecretText = new EditText(context)
            {
                Text = "47eebb1f-b8d0-4c21-9f3a-e8a3ac6e27f4",
                Hint = "Secret..."
            };
            _save = new Button(context)
            {
                Text = "Save"
            };
            _labelServerId = new TextView(context)
            {
                Text = Preferences.Get(OneSmsAction.ServerKey,"Not set"),
            };
            _reconnectToSignalR = new Button(context)
            {
                Text = "Reconnect to Server"
            };
            _labelServerUrl = new TextView(context)
            {
                Text = Preferences.Get(OneSmsAction.ServerUrl, string.Empty),
                TextSize = 20
            };
            _serverUrlText = new EditText(context)
            {
                Hint = "Server URL"
            };
            _serverUrlBtn = new Button(context)
            {
                Text = "Save"
            };
            _labelServer = new TextView(context)
            {
                Text = "Server Url"
            };
            _authBtn = new Button(context)
            {
                Text = "Authenticate"
            };
            _authStateLabel = new TextView(context)
            {
                Text = "Not authenticated",
                TextSize = 20
            };

            Orientation = Orientation.Vertical;
            AddView(_label);
            AddView(_serverText);
            AddView(_labelServerId);
            AddView(_serverSecretText);
            AddView(_save);
            AddView(_authStateLabel);
            AddView(_authBtn);
            AddView(_labelConnected);
            AddView(_reconnectToSignalR);
            AddView(_labelServer);
            AddView(_serverUrlText);
            AddView(_labelServerUrl);
            AddView(_serverUrlBtn);


            _labelConnected.Text = _signalRService.IsConnected ? "Connected" : "Disconnected";
            _save.Click += OnServerIdSave;
            _reconnectToSignalR.Click += async (s, e) =>
            {
                await _signalRService.ReconnectToHub();
                _labelConnected.Text = _signalRService.IsConnected ? "Connected" : "Disconnected";
                
            };
            _serverUrlBtn.Click += (s, e) =>
            {
                if(!string.IsNullOrEmpty(_serverUrlText.Text))
                {
                    Preferences.Set(OneSmsAction.ServerUrl, $"{_serverUrlText.Text}/onesmshub");
                    Preferences.Set(OneSmsAction.BaseUrl, $"{_serverUrlText.Text}/");
                    _labelServerUrl.Text = _serverUrlText.Text;
                    _httpClientService.ChangeBaseAdresse(new Uri(Preferences.Get(OneSmsAction.BaseUrl, string.Empty)));
                    _authService.Authenticate();
                    _signalRService.ChangeUrl(Preferences.Get(OneSmsAction.ServerUrl, string.Empty));
                }
            };

            _signalRService.Connection.Closed += _ => 
            {
                _labelConnected.Text = "Disconnected";
                return Task.CompletedTask;
            };
            _signalRService.Connection.Reconnecting += _ =>
            {
                _labelConnected.Text = "Reconnecting";
                return Task.CompletedTask;
            };
            _signalRService.OnConnectionChanged.Subscribe(con => MainThread.BeginInvokeOnMainThread(() => _labelConnected.Text = con ? "Connected" : "Disconnected"));

            BlobCache.LocalMachine.GetObject<string>(OneSmsAction.AuthKey).Catch(Observable.Return(string.Empty))
                .Subscribe(x => MainThread.BeginInvokeOnMainThread(() => _authStateLabel.Text = string.IsNullOrEmpty(x) ? "Authenticated" : "Authenticated"));

            _authService.OnAuthStateChanged.Subscribe(x =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    _authStateLabel.Text = x ? "Authenticated" : "AuthenticatedAuthenticated";
                });
            });

            _authBtn.Click += (s, e) => _authService.Authenticate();
        }

        private async void OnServerIdSave(object sender, System.EventArgs e)
        {
            if (!string.IsNullOrEmpty(_serverText.Text) && !string.IsNullOrEmpty(_serverSecretText.Text))
            {
                Preferences.Set(OneSmsAction.ServerKey, _serverText.Text);
                Preferences.Set(OneSmsAction.ServerSecret, _serverSecretText.Text);
                _labelServerId.Text = Preferences.Get(OneSmsAction.ServerKey, string.Empty);
                _serverSecretText.Text = string.Empty;
                await _signalRService.ReconnectToHub();
            }
        }
    }
}