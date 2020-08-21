using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Media;
using Android.OS;
using Android.Provider;
using Android.Support.V4.Content;
using Android.Views;
using Android.Widget;
using Java.IO;
using Java.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Environment = Android.OS.Environment;
using File = Java.IO.File;
using FileProvider = Android.Support.V4.Content.FileProvider;
using Uri = Android.Net.Uri;

namespace OneSms.Droid.Server.Views
{
    public class HomeView: LinearLayout
    {
        private const int PICK_IMAGE_REQUSET = 71;
        private Context _context;
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
                    SendWhatsappMessage(_number.Text, _message.Text);
            };
            _buttonChooseImage.Click += (s, e) => ChooseImage();
            _buttonSendImage.Click += async (s, e) =>
            {
                if(!string.IsNullOrEmpty(_number.Text))
                    await CheckContactAndSendImage(context,BitmapImage, _number.Text, _message.Text);
            }; 

        }

        public Bitmap BitmapImage { get; set; }


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
        public async Task CheckContactAndSendImage(Context context,Bitmap bitmap, string number, string message)
        {
            var toNumber = number.Replace("+", "").Replace(" ", "");
            var contactId = await GetContactId(context, toNumber);
            if (!string.IsNullOrEmpty(contactId))
            {
                var whatsappNumber = await GetWhatsappNumber(context, contactId);
                if (string.IsNullOrEmpty(whatsappNumber))
                {
                    //Report number not on whatsapp, send sms
                }
                SendImage(context, bitmap, toNumber, message);
            }
            else
            {
                var contactCreated = await CreateContact(context, number.Replace(" ",""));
                if (contactCreated)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    await CheckContactAndSendImage(context, bitmap, number, message);
                }
                else
                {
                    //Send sms
                }
            }
                
        }
        public void SendImage(Context context,Bitmap bitmap,string number,string message)
        {
            var imgUri = GetLocalBitmapUri(context,bitmap);
            Intent whatsappIntent = new Intent(Intent.ActionSend);
            whatsappIntent.SetType("text/plain");
            whatsappIntent.SetPackage("com.whatsapp.w4b");
            whatsappIntent.PutExtra(Intent.ExtraText, message);
            whatsappIntent.PutExtra(Intent.ExtraStream, imgUri);
            whatsappIntent.PutExtra("jid", $"{number}@s.whatsapp.net");
            whatsappIntent.SetType("image/jpeg");
            whatsappIntent.AddFlags( ActivityFlags.GrantReadUriPermission);

            try
            {
                _context.StartActivity(whatsappIntent);
            }
            catch (Exception ex)
            {
                Toast.MakeText(_context, ex.Message, ToastLength.Long);
            }
        }
        
        public async Task<string> GetContactId(Context context,string number)
        {
            if (await CheckAndRequestReadContactPermission() == PermissionStatus.Granted)
            {
                var contactId = string.Empty;
                if (!string.IsNullOrEmpty(number))
                {
                    var uri = Uri.WithAppendedPath(ContactsContract.PhoneLookup.ContentFilterUri, Uri.Encode(number));
                    var cursor = context.ContentResolver.Query(uri, new string[] { ContactsContract.PhoneLookup.InterfaceConsts.Id }, null, null);
                    if (cursor != null)
                    {
                        while (cursor.MoveToNext())
                            contactId = cursor.GetString(cursor.GetColumnIndexOrThrow(ContactsContract.PhoneLookup.InterfaceConsts.Id));
                        cursor.Close();
                    }
                }
                return contactId;
            }
            return string.Empty;
        }

        public async Task<string> GetWhatsappNumber(Context context,string contactId)
        {
            if (await CheckAndRequestReadContactPermission() == PermissionStatus.Granted)
            {
                var rowContactId = string.Empty;
                var projection = new string[] { ContactsContract.PhoneLookup.InterfaceConsts.Id };
                var selection = $"{ContactsContract.RawContacts.InterfaceConsts.ContactId} = ? AND {ContactsContract.RawContacts.InterfaceConsts.AccountType} = ?";
                var selectionArgs = new string[] { contactId, "com.whatsapp" };
                var cursor = context.ContentResolver.Query(ContactsContract.RawContacts.ContentUri, projection, selection, selectionArgs, null);
                if (cursor != null)
                {
                    bool hasWhatsapp = cursor.MoveToNext();
                    if (hasWhatsapp)
                        rowContactId = cursor.GetString(0);
                    cursor.Close();
                }
                return rowContactId;
            }
            return string.Empty;
        }
        public async Task<bool> CreateContact(Context context, string number)
        {
            if (await CheckAndRequestWriteContactPermission() == PermissionStatus.Granted)
            {
                try
                {
                    var ops = new List<ContentProviderOperation>();
                    int rawContactInsertIndex = ops.Count;

                    ContentProviderOperation.Builder builder =
                        ContentProviderOperation.NewInsert(ContactsContract.RawContacts.ContentUri);
                    builder.WithValue(ContactsContract.RawContacts.InterfaceConsts.AccountType, null);
                    builder.WithValue(ContactsContract.RawContacts.InterfaceConsts.AccountName, null);
                    ops.Add(builder.Build());

                    //Name
                    builder = ContentProviderOperation.NewInsert(ContactsContract.Data.ContentUri);
                    builder.WithValueBackReference(ContactsContract.Data.InterfaceConsts.RawContactId, rawContactInsertIndex);
                    builder.WithValue(ContactsContract.Data.InterfaceConsts.Mimetype,
                        ContactsContract.CommonDataKinds.StructuredName.ContentItemType);
                    builder.WithValue(ContactsContract.CommonDataKinds.StructuredName.FamilyName, "- OneSms");
                    builder.WithValue(ContactsContract.CommonDataKinds.StructuredName.GivenName, number);
                    ops.Add(builder.Build());

                    //Number
                    builder = ContentProviderOperation.NewInsert(ContactsContract.Data.ContentUri);
                    builder.WithValueBackReference(ContactsContract.Data.InterfaceConsts.RawContactId, rawContactInsertIndex);
                    builder.WithValue(ContactsContract.Data.InterfaceConsts.Mimetype,
                        ContactsContract.CommonDataKinds.Phone.ContentItemType);
                    builder.WithValue(ContactsContract.CommonDataKinds.Phone.Number, number);
                    builder.WithValue(ContactsContract.CommonDataKinds.StructuredPostal.InterfaceConsts.Type,
                            ContactsContract.CommonDataKinds.StructuredPostal.InterfaceConsts.TypeCustom);
                    builder.WithValue(ContactsContract.CommonDataKinds.Phone.InterfaceConsts.Label, "Mobile");
                    ops.Add(builder.Build());
                    var res = context.ContentResolver.ApplyBatch(ContactsContract.Authority, ops);
                    return true;
                }
                catch(Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                }
            }
            return false;
        }


        public Uri GetLocalBitmapUri(Context context, Bitmap bitmap)
        {
            
            Uri bmpUri = null;
            try
            {
                string path = System.IO.Path.Combine(Xamarin.Essentials.FileSystem.CacheDirectory, "onesms_sahre" + DateTime.Now.Millisecond + ".png");
                var fs = new FileStream(path, FileMode.OpenOrCreate);
                if (fs != null)
                {
                    bitmap.Compress(Bitmap.CompressFormat.Png, 90, fs);
                    fs.Close();
                }
                File file = new File(path);
                bmpUri = GetUriFromFile(context,file);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
            }
            return bmpUri;
        }

        public static  Uri GetUriFromFile(Context context, File file)
        {
            if (file == null)
                return null;

            if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
            {
                try
                {
                    return FileProvider.GetUriForFile(context.ApplicationContext,  $"{context.PackageName}.fileprovider", file);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e);
                    return null;
                }
            }
            else
            {
                return Uri.FromFile(file);
            }
        }

        public static async Task<PermissionStatus> CheckAndRequestReadStoragePermission()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
            if (status != PermissionStatus.Granted)
                status = await Permissions.RequestAsync<Permissions.StorageRead>();
            return status;
        }

        public static async Task<PermissionStatus> CheckAndRequestWriteStoragePermission()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
            if (status != PermissionStatus.Granted)
                status = await Permissions.RequestAsync<Permissions.StorageWrite>();
            return status;
        }

        public static async Task<PermissionStatus> CheckAndRequestReadContactPermission()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.ContactsRead>();
            if (status != PermissionStatus.Granted)
                status = await Permissions.RequestAsync<Permissions.ContactsRead>();
            return status;
        }

        public static async Task<PermissionStatus> CheckAndRequestWriteContactPermission()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.ContactsWrite>();
            if (status != PermissionStatus.Granted)
                status = await Permissions.RequestAsync<Permissions.ContactsWrite>();
            return status;
        }

    }
}