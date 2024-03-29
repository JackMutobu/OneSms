﻿using Android;
using Android.App;
using Android.Content;
using Android.Service.Notification;
using AndroidX.Core.App;
using OneSms.Contracts.V1.Dtos;
using OneSms.Droid.Server.Constants;
using OneSms.Droid.Server.Extensions;
using Splat;
using System;
using System.Collections.Generic;
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
        private IRequestManagementService _requestManagementService;
        private IWhatsappService _whatsappService;
        private long? _previousNotifWhen;
        private  Dictionary<string, long?> _pkgLastNotificationWhen = new Dictionary<string, long?>();

        public WhatsappNotificationListnerService()
        {
            _whatsappService = Locator.Current.GetService<IWhatsappService>();
            _requestManagementService = Locator.Current.GetService<IRequestManagementService>();
        }

        public override void OnCreate()
        {
            base.OnCreate();
        }

        public override void OnNotificationPosted(StatusBarNotification sbn)
        {
            try
            {
                CancelNotification(sbn.Key);

                if (!(sbn.PackageName == WastappPackageName))
                    return;
                if (_prevNotificationKey == sbn.Key)
                    return;
                else
                    _prevNotificationKey = sbn.Key;

                if ((sbn.Notification.Flags & NotificationFlags.GroupSummary) != 0)
                {
                    Debug.WriteLine(TAG, "Ignore the notification FLAG_GROUP_SUMMARY");
                    return;
                }

                if(_pkgLastNotificationWhen.TryGetValue(sbn.PackageName,out long? lastWhen))
                {
                    if (lastWhen != null && lastWhen >= sbn.Notification.When)
                    {
                        Debug.WriteLine(TAG, "Ignore Old notification");
                        return;
                    }
                }
                _pkgLastNotificationWhen[sbn.PackageName] =  sbn.Notification.When;

                var bundle = sbn.Notification.Extras;
                var from = bundle.GetString(NotificationCompat.ExtraTitle);
                var message = bundle.GetString(NotificationCompat.ExtraText);
                var isGroupMessage = bundle.GetBoolean("android.isGroupConversation");

                if (from == "WhatsApp Business" || message.Contains("new messages") || isGroupMessage)
                    return;

                var androidText = sbn.Notification.Extras.GetString("android.text");
                var receivedMessage = new WhastappMessageReceived
                {
                    Body = message,
                    SenderNumber = from.Contains("OneSms") ? from.Replace("-", "").Replace("OneSms", "").Replace(" ", "") : from,
                    MobileServerKey = Preferences.Get(OneSmsAction.ServerKey, string.Empty),
                    MessageStatus = Contracts.V1.Enumerations.MessageStatus.Received,
                    ReceivedDateTime = DateTime.UtcNow.IgnoreMilliseconds(),
                    CompleteReceivedDateTime = DateTime.UtcNow.IgnoreMilliseconds()
                };

                if (androidText.Contains("📷"))
                    _requestManagementService.OnWhatsappMessageReceived.OnNext(receivedMessage);
                else
                    _whatsappService.ReportReceivedMessage(receivedMessage);
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

       

    }
}