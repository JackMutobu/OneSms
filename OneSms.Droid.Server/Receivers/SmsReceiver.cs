using Android.App;
using Android.Content;
using Android.Widget;
using OneSms.Droid.Server.Constants;
using OneSms.Droid.Server.Enumerations;
using OneSms.Droid.Server.Models;
using OneSms.Droid.Server.Services;
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
            var action = intent.Action;
            var senderNumber = intent.Extras.GetString(OneSmsAction.SenderNumber);
            var receiverNumber = intent.Extras.GetString(OneSmsAction.ReceiverNumber);
            var startTimeStamp = intent.Extras.GetString(OneSmsAction.TimeStamp);
            var smsId = intent.Extras.GetInt(OneSmsAction.SmsId);
            var smsTransaction = new SmsLocalTransaction(senderNumber, receiverNumber, smsId, startTimeStamp, TransactionState.Sent)
            {
                TransactionTime = DateTime.UtcNow
            };
            if (action == OneSmsAction.SmsSent)
            {
                switch(ResultCode)
                {
                    case Result.Ok:
                        _smsService.OnSmsTransaction.OnNext(new SmsLocalTransaction(senderNumber, receiverNumber, smsId, startTimeStamp, TransactionState.Sent));
                        break;
                    case Result.Canceled:
                        var exception = new Exception("Transaction canceled");
                        smsTransaction.TransactionState = TransactionState.Canceled;
                        exception.Data.Add(OneSmsAction.SmsTransaction, smsTransaction);
                        _smsService.OnSmsTransaction.OnError(exception);
                        break;
                }
            }
            else if(action == OneSmsAction.SmsDelivered)
            {
                switch (ResultCode)
                {
                    case Result.Ok:
                        _smsService.OnSmsTransaction.OnNext(new SmsLocalTransaction(senderNumber, receiverNumber, smsId, startTimeStamp, TransactionState.Delivered));
                        _smsService.OnSmsTransaction.OnCompleted();
                        break;
                }
            }
        }
    }
}