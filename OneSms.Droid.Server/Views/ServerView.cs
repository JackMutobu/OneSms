using Android.Content;
using Android.Widget;
using OneSms.Droid.Server.Constants;
using OneSms.Droid.Server.Services;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace OneSms.Droid.Server.Views
{
    public class ServerView:LinearLayout
    {
        private SignalRService _signalRService;
        private TextView _label;
        private TextView _labelServer;
        private TextView _labelServerUrl;
        private EditText _serverUrlText;
        private Button _serverUrlBtn;
        private TextView _labelServerId;
        private EditText _serverText;
        private Button _save;
        private TextView _labelConnected;
        private Button _reconnectToSignalR;
        private Context _context;
        public ServerView(Context context,SignalRService signalRService) : base(context)
        {
            _context = context;
            _signalRService = signalRService;
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
                Hint = "Server ID..."
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
            Orientation = Orientation.Vertical;
            AddView(_label);
            AddView(_serverText);
            AddView(_labelServerId);
            AddView(_save);
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
                if(_signalRService.Connection.State == Microsoft.AspNetCore.SignalR.Client.HubConnectionState.Connected)
                {
                    await _signalRService.Connection.StopAsync(); 
                }
                await _signalRService.ConnectToHub();
                await signalRService.SendServerId(Preferences.Get(OneSmsAction.ServerKey, string.Empty));
                _labelConnected.Text = _signalRService.IsConnected ? "Connected" : "Disconnected";
                
            };
            _serverUrlBtn.Click += (s, e) =>
            {
                if(!string.IsNullOrEmpty(_serverUrlText.Text))
                {
                    var url = $"{_serverUrlText.Text}/onesmshub";
                    Preferences.Set(OneSmsAction.ServerUrl, url);
                    _labelServerUrl.Text = url;
                    _signalRService = new SignalRService(Preferences.Get(OneSmsAction.ServerUrl, string.Empty));
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
            _signalRService.Connection.Reconnected += _ =>
            {
                _labelConnected.Text = "Connected";
                return Task.CompletedTask;
            };
            
        }

        private async void OnServerIdSave(object sender, System.EventArgs e)
        {
            if (!string.IsNullOrEmpty(_serverText.Text))
            {
                Preferences.Set(OneSmsAction.ServerKey, _serverText.Text);
                _labelServerId.Text = Preferences.Get(OneSmsAction.ServerKey, string.Empty);
                await _signalRService.SendServerId(_labelServerId.Text);
            }
        }
    }
}