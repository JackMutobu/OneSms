using Android.App;
using Android.Content;
using Android.OS;
using OneSms.Droid.Server.Constants;
using OneSms.Droid.Server.Services;

namespace OneSms.Droid.Server.Receivers
{
    [BroadcastReceiver]
    [IntentFilter(new string[] { "android.intent.action.BOOT_COMPLETED" }, Priority = int.MaxValue)]
    public class StartReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            if (intent.Action == Intent.ActionBootCompleted)
            {
                var it = new Intent(context, typeof(OneForegroundService));
                it.SetAction(OneSmsAction.StartForegoundService);
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                    context.StartForegroundService(it);
                else
                    context.StartService(it);
            }
        }
    }
}
