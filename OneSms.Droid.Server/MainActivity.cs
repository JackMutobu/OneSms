using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.Core.Content;
using Java.Lang;
using OneSms.Contracts.V1.MobileServerRequest;
using OneSms.Droid.Server.Constants;
using OneSms.Droid.Server.Receivers;
using OneSms.Droid.Server.Services;
using OneSms.Droid.Server.Views;
using OneUssd;
using Splat;
using Syncfusion.Android.TabView;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace OneSms.Droid.Server
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    [IntentFilter(new string[] { "android.intent.action.MAIN" },Categories = new string[] { "android.intent.category.LAUNCHER", "android.intent.category.DEFAULT", "android.intent.category.HOME" }, Priority = int.MaxValue)]
    public class MainActivity : AppCompatActivity
    {
        private SfTabView tabView;
        private SettingsView settingsView;
        private ServerView serverView;
        private HomeView homeView;
        private SmsReceiver smsReceiver;
        private IRequestManagementService requestManagementService;
        private ISmsService smsService;
        private ISignalRService signalRService;
        private IWhatsappService whatsappService;
        private IAuthService authService;


        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            this.Window.AddFlags(WindowManagerFlags.KeepScreenOn);
            Platform.Init(this, savedInstanceState);
            //Register Syncfusion license
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(OneSmsAction.SyncfusionKey);
            //Register Akavache
            Akavache.Registrations.Start("OneSmsDb");

            InitializeServices();
            await RequestPermissions();
            await authService.Authenticate();
            await signalRService.ReconnectToHub();

            tabView = new SfTabView(this.ApplicationContext);
            homeView = new HomeView(this);
            settingsView = new SettingsView(this);
            serverView = new ServerView(this);
            SetContentView(tabView);
            InitializeTab();
        }

        private void InitializeServices()
        {
            Startup.Initialize(this, Preferences.Get(OneSmsAction.BaseUrl, "http://03fa5461216f.ngrok.io/"), Preferences.Get(OneSmsAction.ServerUrl, "http://03fa5461216f.ngrok.io/onesmshub"));
            signalRService = Locator.Current.GetService<ISignalRService>();
            smsService = Locator.Current.GetService<ISmsService>();
            whatsappService = Locator.Current.GetService<IWhatsappService>();
            requestManagementService = Locator.Current.GetService<IRequestManagementService>();
            authService = Locator.Current.GetService<IAuthService>();
            InitializeSmsServices();
        }

        public async Task RequestPermissions()
        {
            UssdController.VerifyAccesibilityAccess(this);
            UssdController.VerifyOverLay(this);
            UssdController.RequestPermission(this);
            await CheckAndRequestReadStorage();
            await smsService.CheckAndRequestSmsPermission();
            await smsService.CheckAndRequestReadPhoneStatePermission();
            await whatsappService.CheckAndRequestReadContactPermission();
            await whatsappService.CheckAndRequestWriteContactPermission();
            whatsappService.NotificationListenerPermission();
        }

        private void InitializeSmsServices()
        {
            smsReceiver = new SmsReceiver();
            CheckForIncommingSMS();
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

        public static void GoToHomeScreen(Context context)
        {
            var startMain = new Intent(Intent.ActionMain);
            startMain.AddCategory(Intent.CategoryHome);
            startMain.AddFlags(ActivityFlags.NewTask);
            context.StartActivity(startMain);
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
            try
            {
                homeView?.SetImageView(homeView?.BitmapImage);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            requestManagementService.OnTransactionCompleted.OnNext(whatsappService?.CurrentTransaction);
        }

        public async Task<PermissionStatus> CheckAndRequestReadStorage()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
            if (status != PermissionStatus.Granted)
                status = await Permissions.RequestAsync<Permissions.StorageRead>();
            if (ContextCompat.CheckSelfPermission(this,Manifest.Permission.ReadExternalStorage) == Permission.Denied)
            {
                //ask for permission
                RequestPermissions(new string[] { Manifest.Permission.ReadExternalStorage }, 67899654);
            }
            return status;
        }



    }
}

