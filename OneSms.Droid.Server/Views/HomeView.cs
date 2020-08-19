using Android.Content;
using Android.Net;
using Android.Views;
using Android.Widget;
using Java.Net;
using System;
using Uri = Android.Net.Uri;

namespace OneSms.Droid.Server.Views
{
    public class HomeView: LinearLayout
    {
        private Context _context;
        private TextView _welcomeText;
        private EditText _message;
        private EditText _number;
        private Button _button;
        public HomeView(Context context):base(context)
        {
            _context = context;
            _welcomeText = new TextView(context) { Text = "Welcome", TextSize = 25, TextAlignment = TextAlignment.Gravity, Gravity = GravityFlags.CenterHorizontal };
            _message = new EditText(context) { Hint = "Message" };
            _number = new EditText(context) { Hint = "Phone Number" };
            _button = new Button(context) { Text = "Send",TextSize=20 };
            Orientation = Orientation.Vertical;
            AddView(_welcomeText);
            AddView(_number);
            AddView(_message);
            AddView(_button);

            _button.Click += (s, e) =>
            {
                if (!string.IsNullOrEmpty(_message.Text) && !string.IsNullOrEmpty(_number.Text))
                    SendWhatsappMessage(_number.Text, _message.Text);
            };

        }

        private void SendWhatsappMessage(string number, string message)
        {
            var packageManager = _context.PackageManager;
            Intent i = new Intent(Intent.ActionView);

            try
            {
                var url = "https://api.whatsapp.com/send?phone=" + number + "&text=" + URLEncoder.Encode(message, "UTF-8");
                i.SetPackage("com.whatsapp.w4b");
                i.SetData(Uri.Parse(url));
                if (i.ResolveActivity(packageManager) != null)
                {
                    _context.StartActivity(i);
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine($"{e.Message}\n { e.StackTrace}");
            }
        }
    }
}