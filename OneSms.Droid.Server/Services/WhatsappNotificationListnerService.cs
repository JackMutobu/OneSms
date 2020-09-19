using Android;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Service.Notification;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Java.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using OneSms.Contracts.V1.Dtos;
using OneSms.Droid.Server.Constants;
using Splat;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Debug = System.Diagnostics.Debug;
using Environment = Android.OS.Environment;
using File = Java.IO.File;

namespace OneSms.Droid.Server.Services
{
    [Service(Enabled = true, Label = "OneSms Whatsapp notification reader", Permission = Manifest.Permission.BindNotificationListenerService)]
    [IntentFilter(new[] { "android.service.notification.NotificationListenerService" })]
    public class WhatsappNotificationListnerService: NotificationListenerService
    {
        private  const string TAG = "NotificationListener";
        private const string  WastappPackageName = "com.whatsapp.w4b";
        private string _prevNotificationKey;
        private IHttpClientService _httpClientService;

        private IWhatsappService _whatsappService;

        public WhatsappNotificationListnerService()
        {
            _whatsappService = Locator.Current.GetService<IWhatsappService>();
            _httpClientService = Locator.Current.GetService<IHttpClientService>();
        }

        public override void OnCreate()
        {
            base.OnCreate();
        }

        public async override void OnNotificationPosted(StatusBarNotification sbn)
        {
            try
            {
                if (!(sbn.PackageName == WastappPackageName))
                    return;
                if (_prevNotificationKey == sbn.Key)
                    return;
                else
                    _prevNotificationKey = sbn.Key;

                CancelNotification(sbn.Key);

                var bundle = sbn.Notification.Extras;
                var from = bundle.GetString(NotificationCompat.ExtraTitle);
                var message = bundle.GetString(NotificationCompat.ExtraText);
                var isGroupMessage = bundle.GetBoolean("android.isGroupConversation");

                if (from == "WhatsApp Business" || message.Contains("new messages") || isGroupMessage)
                    return;

                var whatsappImageDirectory = Environment.ExternalStorageDirectory.AbsolutePath + "/WhatsApp Business/Media/WhatsApp Business Images/";
                var directory = System.IO.Path.GetDirectoryName(whatsappImageDirectory);
                var files = Directory.GetFiles(whatsappImageDirectory);

                File whatsappMediaDirectory = new File(whatsappImageDirectory);

                File[] mediaDirectories = whatsappMediaDirectory.ListFiles();

                var lastFile = mediaDirectories.LastOrDefault();
                var lastFileDate = lastFile.LastModified();
                var millis = GetTime();

                var receivedMessage = new WhastappMessageReceived
                {
                    Body = message,
                    SenderNumber = from.Contains("OneSms") ? from.Replace("-","").Replace("OneSms","").Replace(" ","") : from,
                    MobileServerKey = Preferences.Get(OneSmsAction.ServerKey, string.Empty)
                };
                
                _whatsappService.ReportReceivedMessage(receivedMessage);

            }
            catch(Exception ex)
            {
                Debug.WriteLine(TAG, "Exception: " + ex.Message);
            }
        }

        public override void OnListenerConnected()
        {
            base.OnListenerConnected();
        }

        private static DateTime JanFirst1970 = new DateTime(1970, 1, 1);
        public static long GetTime()
        {
            return (long)((DateTime.Now - JanFirst1970).TotalMilliseconds + 0.5);
        }

        public async Task<PermissionStatus> CheckAndRequestReadStorage()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
            if (status != PermissionStatus.Granted)
                status = await Permissions.RequestAsync<Permissions.StorageRead>();
            return status;
        }

        private IFormFile GetFileForm(Drawable image)
        {
            var bitmap = ((BitmapDrawable)image).Bitmap;
            var stream = new MemoryStream();
            bitmap.Compress(Bitmap.CompressFormat.Jpeg, 100, stream);
            var fileName = $"image{DateTime.Now.Ticks}";
            var formFile = new FormFile(stream, 0, stream.ToArray().Length, fileName, $"{fileName}.jpeg")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };
            return formFile;
        }

        private IFormFile GetFileForm(Bitmap bitmap)
        {
            var stream = new MemoryStream();
            bitmap.Compress(Bitmap.CompressFormat.Jpeg, 100, stream);
            var fileName = $"image{DateTime.Now.Ticks}";
            var formFile = new FormFile(stream, 0, stream.ToArray().Length, fileName, $"{fileName}.jpeg")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };
            return formFile;
        }

        private void PreviousCode()
        {
            //var keys = bundle.KeySet();

            //var images = bundle.Get("android.reduced.images");
            //var template = bundle.Get("android.template");
            //var messagingStyleUser = (Bundle)bundle.Get("android.messagingStyleUser");
            //var styleKeys = messagingStyleUser.KeySet();
            //var icon = messagingStyleUser.GetBundle("icon");
            //var iconKeys = icon.KeySet();
            //var iconImage = (Bitmap)icon.Get("obj");
            //var extensions = bundle.Get("android.wearable.EXTENSIONS");
            //var infoText = bundle.Get("android.infoText");


            //var whenTime = new DateTime(sbn.Notification.When);
            //var afterTimeOut = new DateTime(sbn.Notification.TimeoutAfter);

            //var image = sbn.Notification.GetLargeIcon();
            //var smallIcon = sbn.Notification.SmallIcon;
            //if (smallIcon != null)
            //{
            //    var resources = PackageManager.GetResourcesForApplication(WastappPackageName);

            //    var smallIconDrawable = resources.GetDrawable(smallIcon.ResId);

            //    var imageDrawable = image.LoadDrawable(this);
            //    var imageFileStream = GetFileForm(imageDrawable);
            //    var uploadedFileSuccess = await _httpClientService.UploadImage(imageFileStream);
            //}
            //if (image != null)
            //{
            //    var imageDrawable = image.LoadDrawable(this);
            //    var imageFileStream = GetFileForm(iconImage);
            //    var uploadedFileSuccess = await _httpClientService.UploadImage(imageFileStream);
            //    receivedMessage.ImageLinks.Add(uploadedFileSuccess?.Url);
            //    receivedMessage.Body = message.Substring(2);
            //}
        }

    }
}