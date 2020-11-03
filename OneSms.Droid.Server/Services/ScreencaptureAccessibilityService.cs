using Android;
using Android.AccessibilityServices;
using Android.App;
using Android.Content;
using Android.Util;
using Android.Views.Accessibility;
using OneSms.Droid.Server.Constants;
using Splat;
using System.Collections.Generic;
using System.Linq;

namespace OneSms.Droid.Server.Services
{
    //[Service(Enabled = true, Label = "OneSms on more screens", Permission = Manifest.Permission.BindAccessibilityService)]
    //[IntentFilter(new[] { "android.accessibilityservice.AccessibilityService" })]
    //[MetaData("android.accessibilityservice", Resource = "@xml/access_service")]
    public class ScreencaptureAccessibilityService : AccessibilityService
    {
        public override void OnAccessibilityEvent(AccessibilityEvent e)
        {
            if(e.PackageName != null)
            {
                if(e.PackageName == "com.android.systemui")
                {
                    var nodes = GetLeaves(e);
                    if(nodes.Count() > 0)
                    {
                        var buttons = nodes.Where(x => x.ClassName == "android.widget.Button");
                        if(buttons.Count() == 2 && buttons.Select(x => x.ContentDescription).All(x => x?.Contains("Expa") == false))
                        {
                            var lastButton = buttons.Last();
                            lastButton.PerformAction(Action.Click);
                           
                        }
                    }
                }
            }
        }

        public override void OnInterrupt()
        {
            throw new System.NotImplementedException();
        }

        protected override void OnServiceConnected()
        {
            base.OnServiceConnected();
            Log.Debug("All Service Accessibility", "onServiceConnected");
       
            AccessibilityServiceInfo info = new AccessibilityServiceInfo
            {
                Flags = AccessibilityServiceFlags.Default,
                PackageNames = null,
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