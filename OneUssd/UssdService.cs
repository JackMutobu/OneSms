using Android;
using Android.AccessibilityServices;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using Android.Views.Accessibility;
using System;
using System.Collections.Generic;
using Action = Android.Views.Accessibility.Action;

namespace OneUssd
{
    [Service(Enabled = true, Permission = Manifest.Permission.BindAccessibilityService)]
    [IntentFilter(new[] { "android.accessibilityservice.AccessibilityService" })]
    [MetaData("android.accessibilityservice", Resource = "@xml/ussd_service")]
    public class UssdService : AccessibilityService
    {
        public static event EventHandler<UssdEventArgs> UssdResponseRecievedEventHandler;
        public static event EventHandler<UssdEventArgs> UssdCompletedEventHandler;
        public static event EventHandler<UssdEventArgs> UssdAbortedEventHandler;

        private static readonly string TAG = nameof(UssdService);
        private static AccessibilityEvent _event;
        
        public static UssdService Instance;

        public override void OnAccessibilityEvent(AccessibilityEvent e)
        {
            Instance = this;
            _event = e;
            Log.Debug(TAG, "onAccessibilityEvent");

            Log.Debug(TAG, $"onAccessibilityEvent: {e.EventType}, class: {e.ClassName}, package: {e.PackageName}, time {e.EventTime}, text {e.Text}");

            if (UssdController.Instance == null || !UssdController.Instance.IsRunning)
                return;
           
            if (LoginView(e) && NotInputText(e))
            {
                string response = e.Text[0].ToString() == "Carrier info" ? e.Text[1].ToString() : e.Text[0].ToString();
                // first view or logView, do nothing, pass / FIRST MESSAGE
                ClickOnButton(e, 0);
                UssdController.Instance.IsRunning = false;
                OnUssdResponseRecieved(new UssdEventArgs(response));
            }
            else if(ProblemView(e) || LoginView(e))
            {
                string response = e.Text[0].ToString() == "Carrier info" ? e.Text[1].ToString() : e.Text[0].ToString();
                ClickOnButton(e, 1);
                OnUssdAborted(new UssdEventArgs(response));
            }
            else if(IsUssdWidget(e))
            {
                string response = e.Text[0].ToString() == "Carrier info" ? e.Text[1].ToString() : e.Text[0].ToString();
                if (response.Contains("\n"))
                    response = response.Substring(response.IndexOf('\n') + 1);
                if(NotInputText(e))
                {
                    // not more input panels / LAST MESSAGE
                    // sent 'OK' button
                    ClickOnButton(e, 0);
                    UssdController.Instance.IsRunning = false;
                    OnUssdCompleted(new UssdEventArgs(response));
                }
                else
                {
                    OnUssdResponseRecieved(new UssdEventArgs(response));
                }
            }
        }

        public static void Send(string text)
        {
            SetTextIntoField(_event, text);
            ClickOnButton(_event, 1);
        }

        public static void Cancel()
        {
            ClickOnButton(_event, 0);
        }

        public static string CancelOperation()
        {
            ClickOnButton(_event, 0);
            UssdController.Instance.IsRunning = false;
            return _event?.Text[0]?.ToString() == "Carrier info" ? _event?.Text[1]?.ToString() : _event?.Text[0]?.ToString();
        }

        private void OnUssdAborted(UssdEventArgs ussdEventArgs)
        {
            UssdAbortedEventHandler?.Invoke(this, ussdEventArgs);
        }

        private void OnUssdCompleted(UssdEventArgs ussdEventArgs)
        {
            UssdCompletedEventHandler?.Invoke(this, ussdEventArgs);
        }

        private void OnUssdResponseRecieved(UssdEventArgs ussdEventArgs)
        {
            UssdResponseRecievedEventHandler?.Invoke(this, ussdEventArgs);
        }

        private static void SetTextIntoField(AccessibilityEvent e, string data)
        {
            Bundle arguments = new Bundle();
            arguments.PutCharSequence(
               AccessibilityNodeInfo.ActionArgumentSetTextCharsequence, data);
            foreach(AccessibilityNodeInfo leaf in GetLeaves(e)) {
            if(leaf.ClassName.Equals("android.widget.EditText") && !leaf.PerformAction(Action.SetText, arguments))
            {
                ClipboardManager clipboardManager = ((ClipboardManager)UssdController.Instance.Context
                        .GetSystemService(Context.ClipboardService));
                if (clipboardManager != null)
                {
                    clipboardManager.PrimaryClip = ClipData.NewPlainText("text", data);
                }

                leaf.PerformAction(Action.Paste);
            }
            }
        }

        protected static bool NotInputText(AccessibilityEvent e)
        {
            var flag = true;
            foreach (var leaf in GetLeaves(e))
                if (leaf.ClassName.Equals("android.widget.EditText"))
                    flag = false;
           return flag;
        }

        private bool IsUssdWidget(AccessibilityEvent e)
        {
            return (e.ClassName.Equals("amigo.app.AmigoAlertDialog") || e.ClassName.Equals("android.app.AlertDialog"));
        }

        protected static void ClickOnButton(AccessibilityEvent e,int index)
        {
            int count = -1;
            foreach(var leaf in GetLeaves(e))
            {
                if(leaf.ClassName.ToString().ToLower().Contains("button"))
                {
                    count++;
                    if (count == index)
                        leaf.PerformAction(Action.Click);
                }
            }
        }

        private static List<AccessibilityNodeInfo> GetLeaves(AccessibilityEvent e)
        {
            List<AccessibilityNodeInfo> leaves = new List<AccessibilityNodeInfo>();
            if (e != null && e.Source != null)
                GetLeaves(leaves, e.Source);
            return leaves;
        }

        private static void GetLeaves(List<AccessibilityNodeInfo> leaves, AccessibilityNodeInfo node)
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

        private bool LoginView(AccessibilityEvent e)
        {
            return IsUssdWidget(e) && UssdController.Instance.Map[UssdController.KeyLogin].Contains(e.Text.ToString());
        }

        private bool ProblemView(AccessibilityEvent e)
        {
            return IsUssdWidget(e) && UssdController.Instance.Map[UssdController.KeyLogin].Contains(e.Text[0].ToString());
        }

        public override void OnInterrupt()
        {
            Log.Debug(TAG, "OnInterrupt");
        }

        protected override void OnServiceConnected()
        {
            base.OnServiceConnected();
            Log.Debug(TAG, "onServiceConnected");
            AccessibilityServiceInfo info = new AccessibilityServiceInfo();
            info.Flags = AccessibilityServiceFlags.Default;
            info.PackageNames = new List<string> { "com.android.phone" };
            info.EventTypes = EventTypes.WindowStateChanged | EventTypes.WindowContentChanged;
            info.FeedbackType = FeedbackFlags.Generic;
            SetServiceInfo(info);
        }
    }
}