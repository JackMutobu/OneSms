using Android.App;
using Android.Content;
using Android.Widget;
using OneSms.Droid.Server.Constants;

namespace OneSms.Droid.Server.Receivers
{
    [BroadcastReceiver]
    [IntentFilter(new string[] { "android.provider.Telephony.SMS_RECEIVED" }, Priority = int.MaxValue)]
    public class SmsReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            var action = intent.Action;
            if (action == OneSmsAction.SmsSent)
            {
                switch(ResultCode)
                {
                    case Result.Ok:
                        Toast.MakeText(context,"SMS sent", ToastLength.Short).Show();
                        break;
                    case Result.Canceled:
                        Toast.MakeText(context, "SMS Canceled", ToastLength.Short).Show();
                        break;
                }
            }
            else if(action == OneSmsAction.SmsDelivered)
            {
                switch (ResultCode)
                {
                    case Result.Ok:
                        Toast.MakeText(context, "SMS Delivered", ToastLength.Short).Show();
                        break;
                    case Result.Canceled:
                        Toast.MakeText(context, "SMS Canceled", ToastLength.Short).Show();
                        break;
                }
            }
        }
    }
}