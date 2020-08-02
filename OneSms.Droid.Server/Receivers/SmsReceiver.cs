using Android.App;
using Android.Content;
using Android.OS;
using OneSms.Droid.Server.Constants;
using OneSms.Droid.Server.Services;
using OneSms.Web.Shared.Dtos;
using OneSms.Web.Shared.Enumerations;
using System;

namespace OneSms.Droid.Server.Receivers
{
    [BroadcastReceiver]
    [IntentFilter(new string[] { "android.provider.Telephony.SMS_RECEIVED" }, Priority = int.MaxValue)]
    public class SmsReceiver : BroadcastReceiver
    {
        private readonly SmsService _smsService;

        public SmsReceiver(SmsService smsService)
        {
            _smsService = smsService;
        }
        public SmsReceiver() { }

        public override void OnReceive(Context context, Intent intent)
        {
            if(intent.Action == OneSmsAction.SmsSent || intent.Action == OneSmsAction.SmsDelivered)
            {
                var smsIdBundle = intent.Extras.GetBundle(OneSmsAction.SmsBundleId);
                var senderNumber = intent.Extras.GetString(OneSmsAction.SenderNumber);
                var receiverNumber = intent.Extras.GetString(OneSmsAction.ReceiverNumber);
                var appId = intent.Extras.GetString(OneSmsAction.AppId);
                var serverId = intent.Extras.GetInt(OneSmsAction.MobileServerId);
                var smsId = smsIdBundle.GetInt(OneSmsAction.SmsTransactionId);
                var smsTransaction = new SmsTransactionDto
                {
                    AppId = new Guid(appId),
                    SenderNumber = senderNumber,
                    ReceiverNumber = receiverNumber,
                    SmsId = smsId,
                    TimeStamp = DateTime.UtcNow,
                    MobileServerId = serverId
                };
                smsTransaction.TransactionState = intent.Action == OneSmsAction.SmsSent ? SmsTransactionState.Sent : SmsTransactionState.Delivered;

                switch (ResultCode)
                {
                    case Result.Ok:
                        _smsService.OnSmsTransaction.OnNext(smsTransaction);
                        break;
                    case Result.Canceled:
                        var exception = new Exception("Transaction canceled");
                        smsTransaction.TransactionState = SmsTransactionState.Canceled;
                        exception.Data.Add(OneSmsAction.SmsTransaction, smsTransaction);
                        _smsService.OnSmsTransaction.OnError(exception);
                        break;
                }
            }
            
        }
    }
}