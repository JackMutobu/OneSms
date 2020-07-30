using Android.Content;
using Android.Widget;
using OneSms.Droid.Server.Services;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace OneSms.Droid.Server.Views
{
    public class SettingsView : LinearLayout
    {
        private TextView _label;
        private Button _save;
        private EditText _appId;
        private EditText _message;
        private SmsService _smsService;
        private PermissionStatus permissionStatus;
        public SettingsView(Context context) : base(context)
        {
            Orientation = Orientation.Vertical;
            _smsService = new SmsService(context);
            _label = new TextView(context)
            {
                Text = "Envoyer un sms"
            };
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
            AddView(_label);
            AddView(_appId);
            AddView(_message);
            AddView(_save);
        }

        private async void Save_Click(object sender, System.EventArgs e)
        {
            permissionStatus = await Permissions.CheckStatusAsync<Permissions.Sms>();
            if(permissionStatus == PermissionStatus.Granted)
            {
                if (!string.IsNullOrEmpty(_message.Text) && !string.IsNullOrEmpty(_appId.Text))
                {
                    _smsService.SendSms(_appId.Text, _message.Text);
                }              
            }
            else
                permissionStatus = await Permissions.RequestAsync<Permissions.Sms>();
        }
        public async Task<PermissionStatus> CheckAndRequestSmsPermission()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.Sms>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Sms>();
            }

            // Additionally could prompt the user to turn on in settings

            return status;
        }
    }
}