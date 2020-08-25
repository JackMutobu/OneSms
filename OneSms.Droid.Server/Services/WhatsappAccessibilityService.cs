﻿using Android;
using Android.AccessibilityServices;
using Android.App;
using Android.Content;
using Android.Util;
using Android.Views.Accessibility;
using Android.Widget;
using OneSms.Droid.Server.Constants;
using Splat;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace OneSms.Droid.Server.Services
{
    [Service(Enabled = true, Label = "OneSms on Whatsapp", Permission = Manifest.Permission.BindAccessibilityService)]
    [IntentFilter(new[] { "android.accessibilityservice.AccessibilityService" })]
    [MetaData("android.accessibilityservice", Resource = "@xml/whatsapp_service")]
    public class WhatsappAccessibilityService : AccessibilityService,IEnableLogger
    {
        private IWhatsappService _whatsappService;
        public WhatsappAccessibilityService()
        {
            _whatsappService = Locator.Current.GetService<IWhatsappService>();
        }
        public override void OnAccessibilityEvent(AccessibilityEvent e)
        {
            var eventTye = e.EventType;
            var currentNode = RootInActiveWindow;
            var nodes = GetLeaves(e);
            var buttons = nodes.Where(x => x.ClassName == "android.widget.Button" || x.ClassName == "android.widget.ImageButton");

            SendTextMessage(nodes, buttons);

            SendImage(nodes, buttons);
        }

        private void SendImage(List<AccessibilityNodeInfo> nodes, IEnumerable<AccessibilityNodeInfo> buttons)
        {
            var viewPager = nodes.FirstOrDefault(x => x.ClassName == "androidx.viewpager.widget.ViewPager");
            var textViewPager = nodes.FirstOrDefault(x => x.ClassName == "android.widget.ImageView" && x.ContentDescription == "Add text");
            var navigateViewPager = nodes.FirstOrDefault(x => x.ClassName == "android.widget.ImageView" && x.ContentDescription == "Navigate up");
            var sendPagerButton = buttons.FirstOrDefault(x => x.ContentDescription == "Send");
            if (sendPagerButton != null && viewPager != null && textViewPager != null && navigateViewPager != null)
            {
                sendPagerButton.PerformAction(Action.Click);
                System.Diagnostics.Debug.WriteLine("Image sent");
                //For images this method is called twice from accessibility, so before going back to home screen check if this is the second call
                var transactionId = Preferences.Get(OneSmsAction.ImageTransaction, 0);
                if (_whatsappService.CurrentTransaction.WhatsappId == transactionId)
                    Preferences.Set(OneSmsAction.ImageTransaction, 0);
                else
                    PerformGlobalAction(GlobalAction.Home);
            }
        }

        private void SendTextMessage(List<AccessibilityNodeInfo> nodes, IEnumerable<AccessibilityNodeInfo> buttons)
        {
            var editText = nodes.FirstOrDefault(x => x.ClassName == "android.widget.EditText");
            var sendButton = buttons.FirstOrDefault(x => x.ContentDescription == "Send");
            if (!string.IsNullOrEmpty(editText?.Text) && sendButton != null)
            {
                sendButton.PerformAction(Action.Click);
                System.Diagnostics.Debug.WriteLine("Text sent");
                this.PerformGlobalAction(GlobalAction.Home);
            }
        }

        public override void OnInterrupt()
        {
            this.Log().Warn("Whatsapp Accessibility Interupted");
            return;
        }

        protected override void OnServiceConnected()
        {
            base.OnServiceConnected();
            Log.Debug("Whatsapp Accessibility", "onServiceConnected");
            this.Log().Info("Whatsapp Accessibility Started");
            AccessibilityServiceInfo info = new AccessibilityServiceInfo
            {
                Flags = AccessibilityServiceFlags.Default,
                //com.whatsapp
                PackageNames = new List<string> { "com.whatsapp.w4b" },
                EventTypes = EventTypes.WindowStateChanged | EventTypes.WindowContentChanged,
                FeedbackType = FeedbackFlags.Generic
            };
            SetServiceInfo(info);
        }

        private List<AccessibilityNodeInfo> GetLeaves(AccessibilityEvent e)
        {
            List<AccessibilityNodeInfo> leaves = new List<AccessibilityNodeInfo>();
            if (e != null && e.Source != null)
                GetLeaves(leaves, e.Source);
            return leaves;
        }

        private void GetLeaves(List<AccessibilityNodeInfo> leaves, AccessibilityNodeInfo node)
        {
            if (node?.ChildCount == 0)
            {
                leaves.Add(node);
                return;
            }

            for (int i = 0; i < node?.ChildCount; i++)
            {
                GetLeaves(leaves, node?.GetChild(i));
            }
        }
        public void OpenMainActivity(Context context)
        {
            var intent = new Intent(context, typeof(MainActivity));
            intent.SetFlags(ActivityFlags.NewTask);
            intent.SetAction(Intent.ActionMain);
            intent.AddCategory(Intent.CategoryLauncher);
            context.StartActivity(intent);
        }
    }
}