using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using OneSms.Droid.Server.Constants;
using System;
using System.Threading.Tasks;
using OneSms.Droid.Server.Extensions;
using static Android.Provider.Settings;

namespace OneSms.Droid.Server.Services
{
    [Service(Enabled = true,Exported =false)]
    public class OneForegroundService : Service
    {
        private PowerManager.WakeLock _wakeLock;
        private bool _isServiceStarted;
        private HttpClientService _httpClientService;
        static readonly int NOTIFICATION_ID = 1000;
        static readonly string CHANNEL_ID = "location_notification";
        internal static readonly string COUNT_KEY = "count";

        public OneForegroundService()
        {
            _httpClientService = new HttpClientService("https://jsonplaceholder.typicode.com/");
            
        }

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            if (intent != null)
            {
                switch(intent.Action)
                {
                    case OneSmsAction.StartForegoundService:
                        StartService();
                        break;
                    case OneSmsAction.StopForegoundService:
                        StopService();
                        break;
                }
                
            }
            return StartCommandResult.Sticky;
        }

        public override void OnCreate()
        {
            base.OnCreate();
            System.Diagnostics.Debug.WriteLine("Service started");
            var notification = CreateNotification();
            StartForeground(1, notification);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            System.Diagnostics.Debug.WriteLine("Service destroyed");
            Toast.MakeText(this, "Service destroyed", ToastLength.Short).Show();
        }

        private void StartService()
        {
            if (_isServiceStarted) return;
            System.Diagnostics.Debug.WriteLine("Starting the foreground service task");
            Toast.MakeText(this, "Service starting its task", ToastLength.Short).Show();
            _isServiceStarted = true;
            this.SetServiceState(OneSmsAction.ServiceStarted);

        // we need this lock so our service gets not affected by Doze Mode
            var powerManager = (PowerManager)GetSystemService(PowerService);
            _wakeLock = powerManager.NewWakeLock(WakeLockFlags.Partial, "OneSms::lock");

            // we're starting a loop in a coroutine
            Task.Run(async () =>
            {
                while(_isServiceStarted)
                {
                    await PingFakeServer();
                    await Task.Delay(TimeSpan.FromMinutes(2));
                }
                   
                System.Diagnostics.Debug.WriteLine("End of the loop for the service");
            });
        }

        private void StopService()
        {
            System.Diagnostics.Debug.WriteLine("Stopping the foreground service");
            Toast.MakeText(this, "Service stopping", ToastLength.Long).Show();
            try
            {
                if (_wakeLock?.IsHeld == true)
                    _wakeLock.Release();
                StopForeground(true);
                StopSelf();
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"End of the loop for the service, {ex.Message}");
            }
            finally
            {
                _isServiceStarted = false;
                this.SetServiceState(OneSmsAction.ServiceStopped);
            }
            
        }

        private async Task PingFakeServer()
        {
            var deviceId = Secure.GetString(ApplicationContext.ContentResolver, Secure.AndroidId);
            var dateTimeNow = DateTime.Now.ToString("yyyy’-‘MM’-‘dd’T’HH’:’mm’:’ss.fffffffK");
            var jsonObject = new { DeviceId = deviceId, CreatedAt = dateTimeNow };
            try
            {
                var result = await _httpClientService.PostAsync<string>(jsonObject, "posts");
                if (result.IsSuccess)
                {
                    System.Diagnostics.Debug.WriteLine($"Response {result.Value}");
                    Toast.MakeText(this, $"Response {result.Value}", ToastLength.Long).Show();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Response {result.Message}");
                    Toast.MakeText(this, $"Response {result.Message}", ToastLength.Long).Show();
                }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"End of the loop for the service, {ex.Message}");
            }
       
        }



        private Notification CreateNotification()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var notificationManager = GetSystemService(NotificationService) as NotificationManager;
                var name = Resources.GetString(Resource.String.channel_name);
                var description = GetString(Resource.String.channel_description);
                var channel = new NotificationChannel(CHANNEL_ID, name, NotificationImportance.High) 
                { 
                    Description = description,
                    LightColor = Color.Red,
                    LockscreenVisibility = NotificationVisibility.Public
                };
                channel.EnableLights(true);
                channel.SetVibrationPattern(new long[] { 100, 200, 300, 400, 500, 400, 300, 200, 400 });
                notificationManager.CreateNotificationChannel(channel);
            }
            var pendingIntent = PendingIntent.GetActivity(this, NOTIFICATION_ID, new Intent(this, typeof(MainActivity)), 0);
            var builder = Build.VERSION.SdkInt >= BuildVersionCodes.O ? new Notification.Builder(this, CHANNEL_ID) : new Notification.Builder(this);

            return builder.SetContentTitle("OneSms Service")
                .SetContentText("This is your favorite endless service working")
                .SetContentIntent(pendingIntent)
                .SetTicker("Ticker text")
                .Build();

        }

    }
}