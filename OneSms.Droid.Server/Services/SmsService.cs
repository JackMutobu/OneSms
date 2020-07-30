using Android.App;
using Android.Content;
using Android.Telephony;
using OneSms.Droid.Server.Constants;
using OneSms.Droid.Server.Receivers;
using System.Collections.Generic;
using System.Linq;

namespace OneSms.Droid.Server.Services
{
    public class SmsService
    {
        private Context _context;

        public SmsService(Context context)
        {
            _context = context;
        }
        public void SendSms(string number, string message)
        {
            SmsManager sm = SmsManager.Default;
            List<string> parts = sm.DivideMessage(message).ToList();

            Intent iSent = new Intent(OneSmsAction.SmsSent);
            PendingIntent piSent = PendingIntent.GetBroadcast(_context, 0, iSent, 0);
            Intent iDel = new Intent(OneSmsAction.SmsDelivered);
            PendingIntent piDel = PendingIntent.GetBroadcast(_context, 0, iDel, 0);

            if (parts.Count == 1)
            {
                message = parts[0];
                sm.SendTextMessage(number, null, message, piSent, piDel);
            }
            else
            {
                var sentPis = new List<PendingIntent>();
                var delPis = new List<PendingIntent>();

                for (int i = 0; i < parts.Count; i++)
                {
                    sentPis.Insert(i,piSent);
                    sentPis.Insert(i, piDel);
                }

                sm.SendMultipartTextMessage(number, null, parts, sentPis, delPis);
            }
        }
    }
}