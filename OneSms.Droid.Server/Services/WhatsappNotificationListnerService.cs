using Android;
using Android.App;
using Android.Graphics;
using Android.Service.Notification;
using AndroidX.Core.App;
using OneSms.Contracts.V1.Dtos;
using OneSms.Droid.Server.Constants;
using Splat;
using System;
using System.Diagnostics;
using Xamarin.Essentials;

namespace OneSms.Droid.Server.Services
{
    [Service(Enabled = true, Label = "OneSms Whatsapp notification reader", Permission = Manifest.Permission.BindNotificationListenerService)]
    [IntentFilter(new[] { "android.service.notification.NotificationListenerService" })]
    public class WhatsappNotificationListnerService: NotificationListenerService
    {
        private  const string TAG = "NotificationListener";
        private const string  WastappPackageName = "com.whatsapp.w4b";

        private IWhatsappService _whatsappService;

        public WhatsappNotificationListnerService()
        {
            _whatsappService = Locator.Current.GetService<IWhatsappService>();
        }

        public override void OnCreate()
        {
            base.OnCreate();
        }

        public override void OnNotificationPosted(StatusBarNotification sbn)
        {
            if (!(sbn.PackageName == WastappPackageName)) return;
            var bundle = sbn.Notification.Extras;

            var from = bundle.GetString(NotificationCompat.ExtraTitle);
            var message = bundle.GetString(NotificationCompat.ExtraText);
            var image = bundle.GetByte(NotificationCompat.ExtraPicture);
            var bigLargeIcon = bundle.GetByte(NotificationCompat.ExtraLargeIconBig);
            var bigIcon = bundle.GetByte(NotificationCompat.ExtraLargeIcon);

            int iconId = bundle.GetInt(Notification.ExtraSmallIcon);

            try
            {
                var manager = PackageManager;
                var resources = manager.GetResourcesForApplication(WastappPackageName);

                var icon = resources.GetDrawable(iconId);

            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }

            if (bundle.ContainsKey(Notification.ExtraPicture))
            {
                // this bitmap contain the picture attachment
                var bmp = (Bitmap)bundle.Get(Notification.ExtraPicture);
            }

            _whatsappService.ReportReceivedMessage(new WhastappMessageReceived
            {
                Body = message,
                SenderNumber = from,
                MobileServerKey = Preferences.Get(OneSmsAction.ServerKey, string.Empty)
            }); ;

            Debug.WriteLine(TAG, "From: " + from);
            Debug.WriteLine(TAG, "Message: " + message);
        }

        public override void OnListenerConnected()
        {
            base.OnListenerConnected();
        }
    }
}