using Android;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Service.Notification;
using AndroidX.Core.App;
using OneSms.Contracts.V1.Dtos;
using OneSms.Droid.Server.Constants;
using Splat;
using System;
using System.Linq;
using Xamarin.Essentials;
using Debug = System.Diagnostics.Debug;

namespace OneSms.Droid.Server.Services
{
    [Service(Enabled = true, Label = "OneSms Whatsapp notification reader", Permission = Manifest.Permission.BindNotificationListenerService)]
    [IntentFilter(new[] { "android.service.notification.NotificationListenerService" })]
    public class WhatsappNotificationListnerService : NotificationListenerService
    {
        private const string TAG = "NotificationListener";
        private const string WastappPackageName = "com.whatsapp.w4b";
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

        public override void OnNotificationPosted(StatusBarNotification sbn)
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

                var androidText = sbn.Notification.Extras.GetString("android.text");

                if (androidText.Contains("📷"))
                {
                    //It's a picture
                    var mes = message;
                    OpenNumber(from);
                }
                else
                {
                    var mes = message;
                }
                var receivedMessage = new WhastappMessageReceived
                {
                    Body = message,
                    SenderNumber = from.Contains("OneSms") ? from.Replace("-", "").Replace("OneSms", "").Replace(" ", "") : from,
                    MobileServerKey = Preferences.Get(OneSmsAction.ServerKey, string.Empty)
                };


            }
            catch (Exception ex)
            {
                Debug.WriteLine(TAG, "Exception: " + ex.Message);
            }
        }

        public override void OnListenerConnected()
        {
            Console.WriteLine("Notification listener connected");
            base.OnListenerConnected();
        }

        public void OpenNumber(string number)
        {
            Intent i = new Intent(Intent.ActionView);
            try
            {
                var url = $"https://api.whatsapp.com/send?phone={number}";
                i.SetPackage("com.whatsapp.w4b");
                i.SetData(Android.Net.Uri.Parse(url));
                i.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
                this.StartActivity(i);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine($"{e.Message}\n { e.StackTrace}");
            }
        }

    }
}