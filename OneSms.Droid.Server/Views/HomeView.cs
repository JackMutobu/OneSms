using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using OneSms.Droid.Server.Services;
using Splat;
using System.Text;

namespace OneSms.Droid.Server.Views
{
    public class HomeView: LinearLayout
    {
        private const int PICK_IMAGE_REQUSET = 71;
        private Context _context;
        private IWhatsappService _whatsappService;
        private ISignalRService _signalRService;
        private TextView _welcomeText;
        private EditText _message;
        private EditText _number;
        private Button _button;
        private Button _buttonSendImage;
        private Button _buttonChooseImage;
        private Button _sendVCard;
        private Button _buttonStartSignalRChecker;
        private Button _buttonRestartActivity;
        private ImageView _imageView;
        public HomeView(Context context):base(context)
        {
            _context = context;
            _whatsappService = Locator.Current.GetService<IWhatsappService>();
            _signalRService = Locator.Current.GetService<ISignalRService>();
            _welcomeText = new TextView(context) { Text = "Welcome", TextSize = 25, TextAlignment = TextAlignment.Gravity, Gravity = GravityFlags.CenterHorizontal };
            _message = new EditText(context) { Hint = "Message" };
            _number = new EditText(context) { Hint = "Phone Number",Text = "+254786408335" };
            _button = new Button(context) { Text = "Send"};
            _buttonChooseImage = new Button(context) { Text = "Choose image" };
            _buttonSendImage = new Button(context) { Text = "Send Image" };
            _buttonStartSignalRChecker = new Button(context) { Text = "Start SignalR Checker" };
            _buttonRestartActivity = new Button(context) { Text = "Restart" };
            _sendVCard = new Button(context) { Text = "Send Vcard" };
            _imageView = new ImageView(context);
            Orientation = Android.Widget.Orientation.Vertical;
            AddView(_welcomeText);
            AddView(_number);
            AddView(_message);
            AddView(_button);
            AddView(_buttonChooseImage);
            AddView(_imageView);
            AddView(_buttonSendImage);
            AddView(_sendVCard);
            AddView(_buttonStartSignalRChecker);
            AddView(_buttonRestartActivity);

            _button.Click += (s, e) =>
            {
                if (!string.IsNullOrEmpty(_message.Text) && !string.IsNullOrEmpty(_number.Text))
                        _whatsappService.SendText(_number.Text, _message.Text);
            };
            _buttonChooseImage.Click += (s, e) => ChooseImage();
            _buttonSendImage.Click += async (s, e) =>
            {
                if(!string.IsNullOrEmpty(_number.Text))
                    await _whatsappService.CheckContactAndSendImage(context,BitmapImage, _number.Text, _message.Text);
            };

            _sendVCard.Click += (s, e) =>
            {
                var builder = new StringBuilder();
                builder.AppendLine("BEGIN:VCARD");
                builder.AppendLine("VERSION:2.1");

                // Name        
                builder.Append("N:").Append("Ets")
                  .Append(";").AppendLine("Jambo");

                // Full name        
                builder.Append("FN:").Append("Ets")
                  .Append(" ").AppendLine("Jambo");

                // Address        
                builder.Append("ADR;HOME;PREF:;;").Append("")
                  .Append(";").Append("").Append(";")
                  .Append("").Append(";").AppendLine("");

                // Other data        
                builder.Append("ORG:").AppendLine("Ets Jambo");
                builder.Append("TITLE:").AppendLine("");
                builder.Append("TEL;WORK;VOICE:").AppendLine("+254743946116");
                builder.Append("TEL;CELL;VOICE:").AppendLine("+254743946116");
                builder.Append("URL:").AppendLine("");
                builder.Append("EMAIL;PREF;INTERNET:").AppendLine("");


                builder.AppendLine("END:VCARD");

                
                _whatsappService.CheckContactAndSendVcard(context, builder.ToString(), "+254786408335", "Card message to send");
            };

            _buttonStartSignalRChecker.Click += (s, e) => _signalRService.SignalRServiceConnectionChecker();

            _buttonRestartActivity.Click += (s, e) => MainActivity.RestartActivity(context);
        }

        public Bitmap BitmapImage { get; set; }

        private void ChooseImage()
        {
            Intent intent = new Intent();
            intent.SetType("image/*");
            intent.SetAction(Intent.ActionGetContent);
           
            ((Activity)_context).StartActivityForResult(Intent.CreateChooser(intent, "Select Picture"), PICK_IMAGE_REQUSET);

        }

        public void SetImageView(Bitmap bitmap)
        {
            _imageView.SetImageBitmap(bitmap);
        }
        
    }
}