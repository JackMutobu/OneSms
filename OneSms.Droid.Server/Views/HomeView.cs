using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using OneSms.Droid.Server.Services;
using Splat;

namespace OneSms.Droid.Server.Views
{
    public class HomeView: LinearLayout
    {
        private const int PICK_IMAGE_REQUSET = 71;
        private Context _context;
        private IWhatsappService _whatsappService;
        private TextView _welcomeText;
        private EditText _message;
        private EditText _number;
        private Button _button;
        private Button _buttonSendImage;
        private Button _buttonChooseImage;
        private ImageView _imageView;
        public HomeView(Context context):base(context)
        {
            _context = context;
            _whatsappService = Locator.Current.GetService<IWhatsappService>();
            _welcomeText = new TextView(context) { Text = "Welcome", TextSize = 25, TextAlignment = TextAlignment.Gravity, Gravity = GravityFlags.CenterHorizontal };
            _message = new EditText(context) { Hint = "Message" };
            _number = new EditText(context) { Hint = "Phone Number",Text = "+254786408335" };
            _button = new Button(context) { Text = "Send"};
            _buttonChooseImage = new Button(context) { Text = "Choose image" };
            _buttonSendImage = new Button(context) { Text = "Send Image" };
            _imageView = new ImageView(context);
            Orientation = Android.Widget.Orientation.Vertical;
            AddView(_welcomeText);
            AddView(_number);
            AddView(_message);
            AddView(_button);
            AddView(_buttonChooseImage);
            AddView(_imageView);
            AddView(_buttonSendImage);

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