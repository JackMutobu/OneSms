using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using OneSms.Droid.Server.Constants;

namespace OneSms.Droid.Server.Services
{
    //[Service(Enabled = true, Exported = false, ForegroundServiceType = ForegroundService.TypeMediaProjection)]
    public class ScreenCaptureService : Service
    {
        private Intent _imageCaptureIntent;

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }
        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            if (intent != null)
            {
                switch (intent.Action)
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
            System.Diagnostics.Debug.WriteLine("Service screen capture started");
        }

        private void StartService()
        {
            _imageCaptureIntent = new Intent(this, typeof(ImageCaptureActivity));
            var notification = CreateNotification();
            StartForeground(1, notification);
            _imageCaptureIntent.AddFlags(ActivityFlags.NewTask);
            _imageCaptureIntent.AddFlags(ActivityFlags.ClearTop);
            _imageCaptureIntent.AddFlags(ActivityFlags.SingleTop);
            _imageCaptureIntent.PutExtra(OneSmsAction.KeepRunning, true);
            StartActivity(_imageCaptureIntent);
        }

        private void StopService()
        {
            StopForeground(true);
            StopSelf();
            _imageCaptureIntent.PutExtra(OneSmsAction.KeepRunning, false);
            StartActivity(_imageCaptureIntent);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            System.Diagnostics.Debug.WriteLine("Service capture destroyed");
            
        }
        private Notification CreateNotification()
        {
            string name = "Screen Capture";
            string CHANNEL_ID = "10000";
            int NOTIFICATION_ID = 3;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var notificationManager = GetSystemService(NotificationService) as NotificationManager;
                var description = GetString(Resource.String.channel_description);
                var channel = new NotificationChannel(CHANNEL_ID, name, NotificationImportance.None)
                {
                    Description = description,
                    LightColor = Color.Red,
                    LockscreenVisibility = NotificationVisibility.Secret
                };
                notificationManager.CreateNotificationChannel(channel);
            }
            var pendingIntent = PendingIntent.GetActivity(this, NOTIFICATION_ID, new Intent(this, typeof(MainActivity)), 0);
            var builder = Build.VERSION.SdkInt >= BuildVersionCodes.O ? new Notification.Builder(this, CHANNEL_ID) : new Notification.Builder(this);

            return builder.SetContentTitle("OneSms Service")
                .SetContentText("Screen capture")
                .SetContentIntent(pendingIntent)
                .SetTicker("Screen capture")
                .Build();

        }
    }
}