using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Views;
using AndroidX.AppCompat.App;
using Java.Lang;
using OneSms.Droid.Server.Constants;
using OneSms.Droid.Server.Extensions;
using OneSms.Droid.Server.Receivers;
using OneSms.Droid.Server.Services;
using OneSms.Droid.Server.Views;
using OneUssd;
using Syncfusion.Android.TabView;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace OneSms.Droid.Server
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    [IntentFilter(new string[] { "android.intent.action.MAIN" }, Priority = int.MaxValue)]
    public class MainActivity : AppCompatActivity
    {
        private SfTabView tabView;
        private SettingsView settingsView;
        private ServerView serverView;
        private HomeView homeView;
        private SmsReceiver smsReceiver;
        private SmsService smsService;
        private SignalRService signalRService;
        private HttpClientService httpClientService;

        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            this.Window.AddFlags(WindowManagerFlags.KeepScreenOn);
            Platform.Init(this, savedInstanceState);
            //Register Syncfusion license
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mjk2NzM0QDMxMzgyZTMyMmUzMENBcnhhYldQMkZMbGorVlI4OXhBWUlYOFk1RVV6K0cvNHI2UFFvUGsyVHc9");
            tabView = new SfTabView(this.ApplicationContext);
            httpClientService = new HttpClientService(Preferences.Get(OneSmsAction.BaseUrl, "http://afrisofttech-001-site20.btempurl.com/api/"));
            await InitializeSignalR();
            InitializeSmsServices();
            homeView = new HomeView(this);
            settingsView = new SettingsView(this, smsService, signalRService, httpClientService);
            serverView = new ServerView(this, signalRService, httpClientService);
            await RequestPermissions();
            SetContentView(tabView);
            InitializeTab();
            ActionOnOneForegroundService(OneSmsAction.ServiceStarted);
        }

        private async Task RequestPermissions()
        {
            UssdController.VerifyAccesibilityAccess(this);
            UssdController.VerifyOverLay(this);
            UssdController.RequestPermission(this);
            await SmsService.CheckAndRequestReadPhoneStatePermission();
            await SmsService.CheckAndRequestSmsPermission();
            await HomeView.CheckAndRequestReadContactPermission();
            await HomeView.CheckAndRequestWriteContactPermission();
        }

        

        private void InitializeSmsServices()
        {
            smsService = new SmsService(this,signalRService,httpClientService);
            smsReceiver = new SmsReceiver(smsService);
            CheckForIncommingSMS();
        }

        private async Task InitializeSignalR()
        {
            if (!Preferences.ContainsKey(OneSmsAction.ServerUrl))
                Preferences.Set(OneSmsAction.ServerUrl, "http://afrisofttech-001-site20.btempurl.com/onesmshub");

            signalRService = new SignalRService(this, Preferences.Get(OneSmsAction.ServerUrl, string.Empty));
            await signalRService.ConnectToHub();
            if (Preferences.ContainsKey(OneSmsAction.ServerKey))
                await signalRService.SendServerId(Preferences.Get(OneSmsAction.ServerKey, string.Empty));

        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
        
        public void InitializeTab()
        {
            var tabItems = new TabItemCollection
                {
                    new SfTabItem()
                    {
                        Title = "OneSms",
                        Content = homeView
                    },
                    new SfTabItem()
                    {
                        Title = "Servers",
                        Content = serverView
                    },
                     new SfTabItem()
                    {
                        Title = "Settings",
                        Content = settingsView
                    }
                };
            tabView.Items = tabItems;
            tabView.VisibleHeaderCount = 3;
            tabView.ContentTransitionDuration = 200;
            tabView.EnableSwiping = true;
        }

        private void CheckForIncommingSMS()
        {
            System.Diagnostics.Debug.WriteLine("Sms Receiver registered");
            var intent = new IntentFilter();
            intent.AddAction(OneSmsAction.SmsSent);
            intent.AddAction(OneSmsAction.SmsDelivered);
            this.RegisterReceiver(smsReceiver, intent);
        }

        public static void RestartActivity(Context context)
        {
            PackageManager packageManager = context.PackageManager;
            Intent intent = packageManager.GetLaunchIntentForPackage(context.PackageName);
            ComponentName componentName = intent.Component;
            Intent mainIntent = Intent.MakeRestartActivityTask(componentName);
            context.StartActivity(mainIntent);
            Runtime.GetRuntime().Exit(0);
        }

        public void StartOneForegroundService()
        {
            var it = new Intent(this, typeof(OneForegroundService));
            it.SetAction(OneSmsAction.StartForegoundService);
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                this.StartForegroundService(it);
            else
                this.StartService(it);
        }

        private void ActionOnOneForegroundService(string serviceState)
        {
            if (this.GetServiceState() == OneSmsAction.ServiceStopped && serviceState == OneSmsAction.ServiceStopped) return;
            StartOneForegroundService();
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode == 71 && resultCode == Result.Ok && data != null && data.Data != null)
            {
                var filePath = data.Data;
                try
                {
                    homeView.BitmapImage = MediaStore.Images.Media.GetBitmap(ContentResolver, filePath);
                    byte[] bitmapData;
                    using var stream = new MemoryStream();
                    homeView.BitmapImage.Compress(Bitmap.CompressFormat.Jpeg, 100, stream);
                    bitmapData = stream.ToArray();
                }
                catch (IOException ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                }
            }
            else if (requestCode == 0 && resultCode == Result.Ok && data != null)
            {
                try
                {
                   homeView.BitmapImage = (Bitmap)data.Extras.Get("data");
                    byte[] bitmapData;
                    using var stream = new MemoryStream();
                    homeView.BitmapImage.Compress(Bitmap.CompressFormat.Jpeg, 100, stream);
                    bitmapData = stream.ToArray();
                }
                catch (IOException ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                }
            }
            homeView.SetImageView(homeView.BitmapImage);
        }
    }
}

