using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Telecom;
using Android.Text;
using Android.Views.Accessibility;
using Android.Widget;
using OneUssd.Listeners;
using System;
using System.Collections.Generic;
using Uri = Android.Net.Uri;

namespace OneUssd
{
    public class UssdController : IUssdController
    {
        private Intent _splashLoading;

        public event EventHandler<UssdEventArgs> ResponseRecieved;
        public event EventHandler<UssdEventArgs> SessionCompleted;
        public event EventHandler<UssdEventArgs> SessionAborted;

        public bool IsRunning { get; set; }
        public static string KeyLogin { get; private set; } = "KEY_LOGIN";
        public static string KeyError { get; private set; } = "KEY_ERROR";
        public Dictionary<string, HashSet<string>> Map { get; private set; }
        public static UssdController Instance { get; private set; }
        public Context Context { get; private set; }

        private UssdController(Context context)
        {
            this.Context = context;
            _splashLoading = new Intent(context, typeof(SplashLoadingService));
            RequestPermission();
            UssdService.UssdCompletedEventHandler += UssdService_UssdCompletedEventHandler;
            UssdService.UssdAbortedEventHandler += UssdService_UssdAbortedEventHandler;
            UssdService.UssdResponseRecievedEventHandler += UssdService_UssdResponseRecievedEventHandler;
        }

        public static UssdController GetInstance(Context context)
        {
            if (Instance == null)
                Instance = new UssdController(context);
            return Instance;
        }

        public void CallUSSDInvoke(string ussdPhoneNumber, int simSlot, Dictionary<string, HashSet<string>> map)
        {
            Map = map;
            if (VerifyAccesibilityAccess(Context))
                DialUp(ussdPhoneNumber, simSlot);
            else
                SessionAborted?.Invoke(this, new UssdEventArgs("Check your accessibility"));
        }

        public void CallUSSDOverlayInvoke(string ussdPhoneNumber, int simSlot, Dictionary<string, HashSet<string>> map)
        {
            Map = map;
            if (VerifyAccesibilityAccess(Context) && VerifyOverLay(Context))
                DialUp(ussdPhoneNumber, simSlot);
            else
                SessionAborted?.Invoke(this, new UssdEventArgs("Check your accessibility or overlay permission"));
        }

        private void DialUp(string ussdPhoneNumber, int simSlot)
        {
            if (Map == null || (!Map.ContainsKey(KeyError) || !Map.ContainsKey(KeyLogin)))
            {
                SessionAborted?.Invoke(this, new UssdEventArgs("Bad Mapping structure"));
                return;
            }
            if (string.IsNullOrEmpty(ussdPhoneNumber))
            {
                SessionAborted?.Invoke(this, new UssdEventArgs("Bad ussd number"));
                return;
            }
            var uri = Uri.Encode("#");
            if (uri != null)
                ussdPhoneNumber = ussdPhoneNumber.Replace("#", uri);
            Uri uriPhone = Uri.Parse("tel:" + ussdPhoneNumber);
            if (uriPhone != null)
                IsRunning = true;
            Context.StartService(_splashLoading);
            Context.StartActivity(GetActionCallIntent(uriPhone, simSlot));
        }

        private Intent GetActionCallIntent(Uri uri, int simSlot)
        {
            // https://stackoverflow.com/questions/25524476/make-call-using-a-specified-sim-in-a-dual-sim-device
            string[] simSlotName = {"extra_asus_dial_use_dualsim","com.android.phone.extra.slot","slot","simslot","sim_slot","subscription",
                "Subscription","phone","com.android.phone.DialingMode","simSlot","slot_id","simId","simnum","phone_type","slotId","slotIdx"};

            Intent intent = new Intent(Intent.ActionCall, uri);
            intent.SetFlags(ActivityFlags.NewTask);
            intent.PutExtra("com.android.phone.force.slot", true);
            intent.PutExtra("Cdma_Supp", true);

            foreach (var s in simSlotName)
                intent.PutExtra(s, simSlot);

            var telecomManager = (TelecomManager)Context.GetSystemService(Context.TelecomService);
            if (telecomManager != null)
            {
                var phoneAccountHandleList = telecomManager.CallCapablePhoneAccounts;
                if (phoneAccountHandleList != null && phoneAccountHandleList.Count > simSlot)
                    intent.PutExtra("android.telecom.extra.PHONE_ACCOUNT_HANDLE", phoneAccountHandleList[simSlot]);
            }

            return intent;
        }

        private void UssdService_UssdAbortedEventHandler(object sender, UssdEventArgs e)
        {
            Context.StopService(_splashLoading);
            SessionAborted?.Invoke(this, e);
        }

        private void UssdService_UssdCompletedEventHandler(object sender, UssdEventArgs e)
        {
            Context.StopService(_splashLoading);
            SessionCompleted?.Invoke(this,e);
        }

        private void UssdService_UssdResponseRecievedEventHandler(object sender, UssdEventArgs e)
        {
            ResponseRecieved?.Invoke(this,e);
        }

        public void SendData(string text)
        {
            UssdService.Send(text);
        }

        public static bool VerifyAccesibilityAccess(Context context)
        {
            bool isEnabled = UssdController.IsAccessiblityServicesEnable(context);
            if (!isEnabled)
            {
                if (context is Activity activity)
                {
                    OpenSettingsAccessibility(ref activity);
                }
                else
                {
                    Toast.MakeText(context, "voipUSSD accessibility service is not enabled", ToastLength.Long).Show();
                }
            }
            return isEnabled;
        }

        public static bool VerifyOverLay(Context context)
        {
            bool m_android_doesnt_grant = Build.VERSION.SdkInt >= BuildVersionCodes.M
                    && !Android.Provider.Settings.CanDrawOverlays(context);
            if (m_android_doesnt_grant)
            {
                if (context is Activity activity)
                {
                    OpenSettingsOverlay(ref activity);
                }
                else
                {
                    Toast.MakeText(context, "Overlay permission have not grant permission.", ToastLength.Long).Show();
                }
                return false;
            }
            else
                return true;
        }

        private static void OpenSettingsAccessibility(ref Activity activity)
        {
            AlertDialog.Builder alertDialogBuilder = new AlertDialog.Builder(activity);
            alertDialogBuilder.SetTitle("USSD Accessibility permission");
            ApplicationInfo applicationInfo = activity.ApplicationInfo;
            int stringId = applicationInfo.LabelRes;

            string name = applicationInfo.LabelRes == 0 ? applicationInfo.NonLocalizedLabel.ToString() : activity.GetString(stringId);
            alertDialogBuilder.SetMessage("You must enable accessibility permissions for the app '" + name + "'");
            alertDialogBuilder.SetCancelable(true);
            alertDialogBuilder.SetNeutralButton("Ok", new DialogInterfaceOnClickListener(activity));

            var alertDialog = alertDialogBuilder.Create();
            if (alertDialog != null)
                alertDialog.Show();
        }

        private static void OpenSettingsOverlay(ref Activity activity)
        {
            AlertDialog.Builder alertDialogBuilder = new AlertDialog.Builder(activity);
            alertDialogBuilder.SetTitle("USSD Overlay permission");
            ApplicationInfo applicationInfo = activity.ApplicationInfo;
            int stringId = applicationInfo.LabelRes;
            var name = applicationInfo.LabelRes == 0 ? applicationInfo.NonLocalizedLabel.ToString() : activity.GetString(stringId);
            alertDialogBuilder.SetMessage("You must allow for the app to appear '" + name + "' on top of other apps.");
            alertDialogBuilder.SetCancelable(true);
            alertDialogBuilder.SetNeutralButton("Ok", new DialogInterfaceOverlayListener(activity));

            var alertDialog = alertDialogBuilder.Create();
            if (alertDialog != null)
                alertDialog.Show();
        }

        protected static bool IsAccessiblityServicesEnable(Context context)
        {
            var am = (AccessibilityManager)context.GetSystemService(Context.AccessibilityService);
            if (am != null)
            {
                foreach (var service in am.InstalledAccessibilityServiceList)
                {
                    if (service.Id.Contains(context.PackageName))
                    {
                        return IsAccessibilitySettingsOn(context, service.Id);
                    }
                }
            }
            return false;
        }

        protected static bool IsAccessibilitySettingsOn(Context context, string service)
        {
            int accessibilityEnabled = 0;
            try
            {
                accessibilityEnabled = Android.Provider.Settings.Secure.GetInt(context.ApplicationContext.ContentResolver, Android.Provider.Settings.Secure.AccessibilityEnabled);
            }
            catch (Android.Provider.Settings.SettingNotFoundException e)
            {
                //
            }
            if (accessibilityEnabled == 1)
            {
                var settingValue = Android.Provider.Settings.Secure.GetString(context.ApplicationContext.ContentResolver, Android.Provider.Settings.Secure.EnabledAccessibilityServices);
                if (settingValue != null)
                {
                    TextUtils.SimpleStringSplitter splitter = new TextUtils.SimpleStringSplitter(':');
                    splitter.SetString(settingValue);
                    while (splitter.HasNext)
                    {
                        var accessabilityService = splitter.Next();
                        if (accessabilityService.Equals(service, System.StringComparison.InvariantCultureIgnoreCase))
                            return true;
                    }
                }
            }
            return false;
        }

        private void RequestPermission()
        {
            if (ContextCompat.CheckSelfPermission(Context, Manifest.Permission.CallPhone) != Permission.Granted)
            {
                ActivityCompat.RequestPermissions(Context as Activity, new string[] { Manifest.Permission.CallPhone }, 1);
            }
            if (ContextCompat.CheckSelfPermission(Context, Manifest.Permission.ReadPhoneState) != Permission.Granted)
            {
                ActivityCompat.RequestPermissions(Context as Activity, new string[] { Manifest.Permission.ReadPhoneState }, 1);
            }

        }
    }
}