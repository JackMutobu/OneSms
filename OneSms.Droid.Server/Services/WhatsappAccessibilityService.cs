using Android;
using Android.AccessibilityServices;
using Android.App;
using Android.Util;
using Android.Views.Accessibility;
using System.Collections.Generic;

namespace OneSms.Droid.Server.Services
{
    [Service(Enabled = true, Label = "OneSms on Whatsapp", Permission = Manifest.Permission.BindAccessibilityService)]
    [IntentFilter(new[] { "android.accessibilityservice.AccessibilityService" })]
    [MetaData("android.accessibilityservice", Resource = "@xml/whatsapp_service")]
    public class WhatsappAccessibilityService : AccessibilityService
    {
        public override void OnAccessibilityEvent(AccessibilityEvent e)
        {
            throw new System.NotImplementedException();
        }

        public override void OnInterrupt()
        {
            throw new System.NotImplementedException();
        }

        protected override void OnServiceConnected()
        {
            base.OnServiceConnected();
            Log.Debug("Whatsapp Accessibility", "onServiceConnected");
            AccessibilityServiceInfo info = new AccessibilityServiceInfo
            {
                Flags = AccessibilityServiceFlags.Default,
                PackageNames = new List<string> { "com.whatsapp.w4b,com.whatsapp" },
                EventTypes = EventTypes.WindowStateChanged | EventTypes.WindowContentChanged,
                FeedbackType = FeedbackFlags.Generic
            };
            SetServiceInfo(info);
        }
    }
}