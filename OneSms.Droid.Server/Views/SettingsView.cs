using Android.Content;
using Android.Widget;
using OneSms.Droid.Server.Services;
using OneUssd;
using Splat;
using System.Collections.Generic;
using System.Linq;

namespace OneSms.Droid.Server.Views
{
    public class SettingsView : LinearLayout
    {
        private TextView _label;
        private TextView _ussdLabel;
        private Button _save;
        private Button _executeUssd;
        private EditText _appId;
        private EditText _message;
        private EditText _simNumber;
        private EditText _ussdCode;
        private EditText _ussdData;
        private ISmsService _smsService;
        private ISignalRService _signalRService;
        private IHttpClientService _httpClientService;
        private IUssdService _ussdService;

        
        public SettingsView(Context context) : base(context)
        {
            Orientation = Orientation.Vertical;
            _smsService = Locator.Current.GetService<ISmsService>();
            _signalRService = Locator.Current.GetService<ISignalRService>();
            _httpClientService = Locator.Current.GetService<IHttpClientService>(); ;
            _ussdService = Locator.Current.GetService<IUssdService>();
            _label = new TextView(context)
            {
                Text = "Envoyer un sms"
            };
            _ussdLabel = new TextView(context);
            _save = new Button(context)
            {
                Text = "Send"
            };
            _message = new EditText(context);
            _save.Click += Save_Click;
            _appId = new EditText(context)
            {
                Hint= "Phone number"
            };
            _message = new EditText(context)
            {
                Hint = "Sms body"
            };
            _simNumber = new EditText(context) 
            {
                Hint = "Sim slot"
            };
            _ussdLabel = new TextView(context)
            {
                Text = "Ussd Transaction"
            };
            _ussdCode = new EditText(context) 
            {
                Hint = "Ussd Code"
            };
            _ussdData = new EditText(context) 
            {
                Hint = "Ussd Data"
            };
            _executeUssd = new Button(context)
            {
                Text = "Execute Ussd",
            };
            AddView(_label);
            AddView(_appId);
            AddView(_message);
            AddView(_simNumber);
            AddView(_save);
            AddView(_ussdLabel);
            AddView(_ussdCode);
            AddView(_ussdData);
            AddView(_executeUssd);
            _executeUssd.Click += OnUssdClick;
        }

        private void OnUssdClick(object sender, System.EventArgs e)
        {
            if (!string.IsNullOrEmpty(_ussdData.Text) && !string.IsNullOrEmpty(_ussdCode.Text))
            {
                var sim = string.IsNullOrEmpty(_simNumber.Text) ? 0 : int.Parse(_simNumber.Text);
                var keyProblems = new HashSet<string>();
                var keyWelcome = new HashSet<string>();
                var data = new Dictionary<string, HashSet<string>>
                {
                    { UssdController.KeyLogin, keyWelcome },
                    { UssdController.KeyError, keyProblems }
                };
                _ussdService.Execute(_ussdCode.Text, sim, data, _ussdData.Text.Split(",").ToList());
            }
        }

        private async void Save_Click(object sender, System.EventArgs e)
        {
            if (!string.IsNullOrEmpty(_message.Text) && !string.IsNullOrEmpty(_appId.Text))
            {
                var sim = string.IsNullOrEmpty(_simNumber.Text) ? 0 : int.Parse(_simNumber.Text);
                await _smsService.SendSms(_appId.Text, _message.Text, sim);
            }
        }
    }
}