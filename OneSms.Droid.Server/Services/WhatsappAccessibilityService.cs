using Android;
using Android.AccessibilityServices;
using Android.App;
using Android.Util;
using Android.Views.Accessibility;
using Android.Widget;
using Splat;
using System.Collections.Generic;
using System.Linq;

namespace OneSms.Droid.Server.Services
{
    [Service(Enabled = true, Label = "OneSms on Whatsapp", Permission = Manifest.Permission.BindAccessibilityService)]
    [IntentFilter(new[] { "android.accessibilityservice.AccessibilityService" })]
    [MetaData("android.accessibilityservice", Resource = "@xml/whatsapp_service")]
    public class WhatsappAccessibilityService : AccessibilityService,IEnableLogger
    {
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
                Toast.MakeText(this, "Image sent", ToastLength.Short);
                System.Diagnostics.Debug.WriteLine("Image sent");
            }
        }

        private void SendTextMessage(List<AccessibilityNodeInfo> nodes, IEnumerable<AccessibilityNodeInfo> buttons)
        {
            var editText = nodes.FirstOrDefault(x => x.ClassName == "android.widget.EditText");
            var sendButton = buttons.FirstOrDefault(x => x.ContentDescription == "Send");
            if (!string.IsNullOrEmpty(editText?.Text) && sendButton != null)
            {
                sendButton.PerformAction(Action.Click);
                Toast.MakeText(this, "Text sent", ToastLength.Short);
                System.Diagnostics.Debug.WriteLine("Text sent");
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
    }
}