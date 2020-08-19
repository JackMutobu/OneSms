using Android;
using Android.AccessibilityServices;
using Android.App;
using Android.Util;
using Android.Views.Accessibility;
using System.Collections.Generic;
using System.Linq;

namespace OneSms.Droid.Server.Services
{
    [Service(Enabled = true, Label = "OneSms on Whatsapp", Permission = Manifest.Permission.BindAccessibilityService)]
    [IntentFilter(new[] { "android.accessibilityservice.AccessibilityService" })]
    [MetaData("android.accessibilityservice", Resource = "@xml/whatsapp_service")]
    public class WhatsappAccessibilityService : AccessibilityService
    {
        public override void OnAccessibilityEvent(AccessibilityEvent e)
        {
            var eventTye = e.EventType;
            var currentNode = RootInActiveWindow;
            var nodes = GetLeaves(e);
            var buttons = nodes.Where(x => x.ClassName == "android.widget.Button" || x.ClassName == "android.widget.ImageButton");
            var editText = nodes.FirstOrDefault(x => x.ClassName == "android.widget.EditText");
            var sendButton = buttons.FirstOrDefault(x => x.ContentDescription == "Send");
            if (!string.IsNullOrEmpty(editText?.Text) && sendButton != null)
                sendButton.PerformAction(Action.Click);
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
            if (node.ChildCount == 0)
            {
                leaves.Add(node);
                return;
            }

            for (int i = 0; i < node.ChildCount; i++)
            {
                GetLeaves(leaves, node.GetChild(i));
            }
        }
    }
}