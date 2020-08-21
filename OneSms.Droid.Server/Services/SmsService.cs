using Android.App;
using Android.Content;
using Android.OS;
using Android.Telephony;
using OneSms.Droid.Server.Constants;
using OneSms.Droid.Server.ExtendedPermissions;
using OneSms.Droid.Server.Models;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Xamarin.Essentials;
using System;
using System.Reactive.Linq;
using OneSms.Web.Shared.Constants;
using OneSms.Web.Shared.Dtos;
using Microsoft.AspNetCore.SignalR.Client;
using System.Text.Json;
using OneSms.Web.Shared.Enumerations;
using OneSms.Web.Shared.Models;

namespace OneSms.Droid.Server.Services
{
    public class SmsService
    {
        private Context _context;
        private SignalRService _signalRService;
        private HttpClientService _httpClientService;
        private Queue<SmsTransactionDto> _pendingSms;

        public SmsService()
        {
            _httpClientService = new HttpClientService(Preferences.Get(OneSmsAction.BaseUrl, "http://afrisofttech-001-site20.btempurl.com/"));
        }
        
        public SmsService(Context context,SignalRService signalRService,HttpClientService httpClientService)
        {
            _context = context;
            _signalRService = signalRService;
            _httpClientService = httpClientService;
            OnSmsTransaction = new Subject<SmsTransactionDto>();
            OnSmsTransaction.Subscribe(sms => 
            {
                _httpClientService.PutAsync<string>(sms, "Sms/StatusChanged");
                if(_pendingSms.Count > 0 && sms.TransactionState == SmsTransactionState.Sent)
                     SendSms(_pendingSms.Dequeue());
            },
            ex => 
            {
                var transacton = ex.Data[OneSmsAction.SmsTransaction] as SmsTransactionDto;
                _httpClientService.PutAsync<string>(transacton, "Sms/StatusChanged");
                if (_pendingSms.Count > 0)
                    SendSms(_pendingSms.Dequeue());
            });

            _signalRService.Connection.On<SmsTransactionDto>(SignalRKeys.SendSms, async sms => 
            {
                if (_pendingSms.Count == 0)
                    await SendSms(sms);
                else
                    _pendingSms.Enqueue(sms);
            });
            _pendingSms = new Queue<SmsTransactionDto>();
        }

        public Subject<SmsTransactionDto> OnSmsTransaction { get; }

        public async Task SendSms(string number, string message,int sim)
        {
            var smsPermission = await CheckAndRequestSmsPermission();
            var readPhoneStatePermission = await CheckAndRequestReadPhoneStatePermission();
            if (smsPermission == PermissionStatus.Granted && readPhoneStatePermission == PermissionStatus.Granted)
            {
                SmsManager sm = SmsManager.Default;
                List<string> parts = sm.DivideMessage(message).ToList();

                Intent iSent = new Intent(OneSmsAction.SmsSent);
                iSent.PutExtra(OneSmsAction.ReceiverNumber,number);
                iSent.PutExtra(OneSmsAction.SmsTransactionId, "482498");

                PendingIntent piSent = PendingIntent.GetBroadcast(_context, int.Parse(number), iSent, 0);
               
                Intent iDel = new Intent(OneSmsAction.SmsDelivered);
                iDel.PutExtra(OneSmsAction.ReceiverNumber, number);
                iDel.PutExtra(OneSmsAction.SmsTransactionId, "482498");
                PendingIntent piDel = PendingIntent.GetBroadcast(_context, 0, iDel, 0);

                if (Build.VERSION.SdkInt >= BuildVersionCodes.LollipopMr1)
                {
                    SubscriptionManager localSubscriptionManager = (SubscriptionManager)_context.GetSystemService(Context.TelephonySubscriptionService);

                    if (localSubscriptionManager.ActiveSubscriptionInfoCount > 1)
                    {
                        var localList = localSubscriptionManager.ActiveSubscriptionInfoList;

                        SubscriptionInfo simInfo1 = localList[0];
                        SubscriptionInfo simInfo2 = localList[1];
                        if (sim == 0)
                        {
                            //SendSMS From SIM One
                            SmsManager.GetSmsManagerForSubscriptionId(simInfo1.SubscriptionId).SendTextMessage(number, null, message, piSent, piDel);
                        }
                        else
                        {
                            //SendSMS From SIM Two
                            SmsManager.GetSmsManagerForSubscriptionId(simInfo2.SubscriptionId).SendTextMessage(number, null, message, piSent, piDel);
                        }
                    }
                }
                else
                {
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
                            sentPis.Insert(i, piSent);
                            sentPis.Insert(i, piDel);
                        }

                        sm.SendMultipartTextMessage(number, null, parts, sentPis, delPis);
                    }
                }
            }

            
        }

        public async Task SendSms(SmsTransactionDto smsTransactionDto)
        {
            var smsPermission = await CheckAndRequestSmsPermission();
            var readPhoneStatePermission = await CheckAndRequestReadPhoneStatePermission();
            if (smsPermission == PermissionStatus.Granted && readPhoneStatePermission == PermissionStatus.Granted)
            {
                SmsManager sm = SmsManager.Default;
                List<string> parts = sm.DivideMessage(smsTransactionDto.Message).ToList();
                var bundle = new Bundle(1);
                bundle.PutInt(OneSmsAction.SmsTransactionId, smsTransactionDto.SmsId);
                Intent iSent = new Intent(OneSmsAction.SmsSent);
                iSent.PutExtra(OneSmsAction.SmsBundleId, bundle);
                iSent.PutExtra(OneSmsAction.ReceiverNumber, smsTransactionDto.ReceiverNumber);
                iSent.PutExtra(OneSmsAction.AppId, smsTransactionDto.AppId.ToString());
                iSent.PutExtra(OneSmsAction.SenderNumber, smsTransactionDto.SenderNumber);
                iSent.PutExtra(OneSmsAction.MobileServerId, smsTransactionDto.MobileServerId);

                PendingIntent piSent = PendingIntent.GetBroadcast(_context, smsTransactionDto.SmsId, iSent, PendingIntentFlags.UpdateCurrent);

                Intent iDel = new Intent(OneSmsAction.SmsDelivered);
                iDel.PutExtra(OneSmsAction.SmsBundleId, bundle);
                iDel.PutExtra(OneSmsAction.ReceiverNumber, smsTransactionDto.ReceiverNumber);
                iDel.PutExtra(OneSmsAction.AppId, smsTransactionDto.AppId.ToString());
                iDel.PutExtra(OneSmsAction.SenderNumber, smsTransactionDto.SenderNumber);
                iDel.PutExtra(OneSmsAction.MobileServerId, smsTransactionDto.MobileServerId);

                PendingIntent piDel = PendingIntent.GetBroadcast(_context, smsTransactionDto.SmsId, iDel, PendingIntentFlags.UpdateCurrent);

                if (Build.VERSION.SdkInt >= BuildVersionCodes.LollipopMr1)
                {
                    SubscriptionManager localSubscriptionManager = (SubscriptionManager)_context.GetSystemService(Context.TelephonySubscriptionService);

                    if (localSubscriptionManager.ActiveSubscriptionInfoCount > 1)
                    {
                        var localList = localSubscriptionManager.ActiveSubscriptionInfoList;

                        SubscriptionInfo simInfo1 = localList[0];
                        SubscriptionInfo simInfo2 = localList[1];
                        if (smsTransactionDto.SimSlot == 0)
                        {
                            //SendSMS From SIM One
                            SmsManager.GetSmsManagerForSubscriptionId(simInfo1.SubscriptionId).SendTextMessage(smsTransactionDto.ReceiverNumber, null, smsTransactionDto.Message, piSent, piDel);
                        }
                        else
                        {
                            //SendSMS From SIM Two
                            SmsManager.GetSmsManagerForSubscriptionId(simInfo2.SubscriptionId).SendTextMessage(smsTransactionDto.ReceiverNumber, null, smsTransactionDto.Message, piSent, piDel);
                        }
                    }
                }
                else
                {
                    if (parts.Count == 1)
                    {
                        smsTransactionDto.Message = parts[0];
                        sm.SendTextMessage(smsTransactionDto.ReceiverNumber, null, smsTransactionDto.Message, piSent, piDel);
                    }
                    else
                    {
                        var sentPis = new List<PendingIntent>();
                        var delPis = new List<PendingIntent>();

                        for (int i = 0; i < parts.Count; i++)
                        {
                            sentPis.Insert(i, piSent);
                            sentPis.Insert(i, piDel);
                        }

                        sm.SendMultipartTextMessage(smsTransactionDto.ReceiverNumber, null, parts, sentPis, delPis);
                    }
                }
            }
        }

        public Task<Result<string>> SendReceivedSms(SmsReceivedDto smsReceivedDto) => _httpClientService.PutAsync<string>(smsReceivedDto, "Sms/SmsReceived");

        public static async Task<PermissionStatus> CheckAndRequestSmsPermission()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.Sms>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Sms>();
            }

            // Additionally could prompt the user to turn on in settings

            return status;
        }

        public static async Task<PermissionStatus> CheckAndRequestReadPhoneStatePermission()
        {
            var status = await Permissions.CheckStatusAsync<ReadPhoneStatePermission>();
            if (status != PermissionStatus.Granted)
                status = await Permissions.RequestAsync<ReadPhoneStatePermission>();
            return status;
        }

        
    }
}