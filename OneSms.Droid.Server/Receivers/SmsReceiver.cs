using Android.App;
using Android.Content;
using Android.OS;
using Android.Provider;
using Android.Util;
using OneSms.Contracts.V1.Dtos;
using OneSms.Contracts.V1.Enumerations;
using OneSms.Contracts.V1.MobileServerRequest;
using OneSms.Droid.Server.Constants;
using OneSms.Droid.Server.Services;
using Splat;
using System;
using Xamarin.Essentials;
using Debug = System.Diagnostics.Debug;

namespace OneSms.Droid.Server.Receivers
{
    [BroadcastReceiver]
    [IntentFilter(new string[] { "android.provider.Telephony.SMS_RECEIVED" }, Priority = int.MaxValue)]
    public class SmsReceiver : BroadcastReceiver
    {
        private ISmsService _smsService;

        public SmsReceiver()
        {
            _smsService = Locator.Current.GetService<ISmsService>();
        }

        public override void OnReceive(Context context, Intent intent)
        {
            if(intent.Action == OneSmsAction.SmsSent || intent.Action == OneSmsAction.SmsDelivered)
            {
                var smsIdBundle = intent.Extras.GetBundle(OneSmsAction.SmsBundleId);
                var senderNumber = intent.Extras.GetString(OneSmsAction.SenderNumber);
                var receiverNumber = intent.Extras.GetString(OneSmsAction.ReceiverNumber);
                var appId = intent.Extras.GetString(OneSmsAction.AppId);
                var serverId = intent.Extras.GetString(OneSmsAction.MobileServerId);
                var smsId = smsIdBundle.GetInt(OneSmsAction.SmsTransactionId);
                var body = smsIdBundle.GetString(OneSmsAction.Body);
                var smsTransaction = new SmsRequest
                {
                    AppId = new Guid(appId),
                    SenderNumber = senderNumber,
                    ReceiverNumber = receiverNumber,
                    MessageId = smsId,
                    MobileServerId = new Guid(serverId),
                    Body = body
                };
                smsTransaction.MessageStatus = intent.Action == OneSmsAction.SmsSent ? MessageStatus.Sent : MessageStatus.Delivered;

                switch (ResultCode)
                {
                    case Result.Ok:
                        _smsService.OnSmsTransaction.OnNext(smsTransaction);
                        break;
                    case Result.Canceled:
                        var exception = new Exception("Transaction canceled");
                        smsTransaction.MessageStatus = MessageStatus.Canceled;
                        exception.Data.Add(OneSmsAction.SmsTransaction, smsTransaction);
                        _smsService.OnSmsTransaction.OnError(exception);
                        break;
                }

                return;
            }
            
            if(intent.Action.Equals(Telephony.Sms.Intents.SmsReceivedAction))
            {
                var smsMessages = Telephony.Sms.Intents.GetMessagesFromIntent(intent);
                var smsModel = new SmsReceived();
                foreach(var sms in smsMessages)
                {
                    smsModel.Body += sms.MessageBody;
                    smsModel.OriginatingAddress = sms.OriginatingAddress.ToLower();
                }
                smsModel.SimSlot = GetSimSlot(intent);
                smsModel.MobileServerKey = Preferences.Get(OneSmsAction.ServerKey,"Server Key not set");
                _smsService ??= new SmsService();
                _smsService.SendReceivedSms(smsModel);
            }
        }
        private int GetSimSlot(Intent intent)
        {
            try
            {
                Bundle bundle = intent.Extras;
                int slot = -1;
                if (bundle != null)
                {
                    var keySet = bundle.KeySet();
                    foreach(var key in keySet)
                    {
                        switch(key)
                        {
                            case "slot":
                                slot = bundle.GetInt("slot", -1);
                                break;
                            case "simId":
                                slot = bundle.GetInt("simId", -1);
                                break;
                            case "simSlot":
                                slot = bundle.GetInt("simSlot", -1);
                                break;
                            case "slot_id":
                                slot = bundle.GetInt("slot_id", -1);
                                break;
                            case "simnum":
                                slot = bundle.GetInt("simnum", -1);
                                break;
                            case "slotId":
                                slot = bundle.GetInt("slotId", -1);
                                break;
                            case "slotIdx":
                                slot = bundle.GetInt("slotIdx", -1);
                                break;
                            default:
                                if (key.ToLower().Contains("slot") | key.ToLower().Contains("sim"))
                                {
                                    var value = bundle.GetString(key, "-1");
                                    if (value.Equals("0") | value.Equals("1") | value.Equals("2"))
                                    {
                                        slot = bundle.GetInt(key, -1);
                                    }
                                }
                                break;
                        }
                    }

                    Log.Debug("slot", "slot=>" + slot);
                    Debug.WriteLine("slot=>" + slot);
                }
                return slot;

            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception=>" + e);
                return 0;
            }
        }
    }
}