using Android.App;
using Android.Content;
using Android.OS;
using Android.Telephony;
using OneSms.Droid.Server.Constants;
using OneSms.Droid.Server.ExtendedPermissions;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Xamarin.Essentials;
using System;
using System.Reactive.Linq;
using Microsoft.AspNetCore.SignalR.Client;
using Splat;
using OneSms.Contracts.V1.MobileServerRequest;
using OneSms.Contracts.V1.Enumerations;
using OneSms.Droid.Server.Models;
using OneSms.Contracts.V1.Dtos;
using OneSms.Contracts.V1;

namespace OneSms.Droid.Server.Services
{
    public interface ISmsService
    {
        Subject<SmsRequest> OnSmsTransaction { get; }

        Task<PermissionStatus> CheckAndRequestReadPhoneStatePermission();
        Task<PermissionStatus> CheckAndRequestSmsPermission();
        Task<Result<string>> SendReceivedSms(SmsReceived smsReceived);
        Task SendSms(SmsRequest smsTransactionDto);
        Task SendSms(string number, string message, int sim);
        Context Context { get; set; }
    }

    public class SmsService : ISmsService
    {
        private ISignalRService _signalRService;
        private IHttpClientService _httpClientService;
        private Queue<SmsRequest> _pendingSms;

        public SmsService()
        {
            _httpClientService = Locator.Current.GetService<IHttpClientService>();
        }

        public SmsService(Context context)
        {
            Context = context;
            _signalRService = Locator.Current.GetService<ISignalRService>();
            _httpClientService = Locator.Current.GetService<IHttpClientService>();
            OnSmsTransaction = new Subject<SmsRequest>();
            OnSmsTransaction.Subscribe(sms =>
            {
                _httpClientService.PutAsync<string>(sms, ApiRoutes.Sms.StatusChanged);
                if (_pendingSms.Count > 0 && sms.MessageStatus == MessageStatus.Sent)
                    SendSms(_pendingSms.Dequeue());
            },
            ex =>
            {
                var transacton = ex.Data[OneSmsAction.SmsTransaction] as SmsRequest;
                _httpClientService.PutAsync<string>(transacton, ApiRoutes.Sms.StatusChanged);
                if (_pendingSms.Count > 0)
                    SendSms(_pendingSms.Dequeue());
            });

            _signalRService.Connection.On<SmsRequest>(SignalRKeys.SendSms,sms =>
            {
                if (_pendingSms.Count == 0)
                     SendSms(sms);
                else
                    _pendingSms.Enqueue(sms);
            });
            _pendingSms = new Queue<SmsRequest>();
        }

        public Subject<SmsRequest> OnSmsTransaction { get; }

        public Context Context { get; set; }

        public async Task SendSms(string number, string message, int sim)
        {
            var smsPermission = await CheckAndRequestSmsPermission();
            var readPhoneStatePermission = await CheckAndRequestReadPhoneStatePermission();
            if (smsPermission == PermissionStatus.Granted && readPhoneStatePermission == PermissionStatus.Granted)
            {
                SmsManager sm = SmsManager.Default;
                List<string> parts = sm.DivideMessage(message).ToList();

                Intent iSent = new Intent(OneSmsAction.SmsSent);
                iSent.PutExtra(OneSmsAction.ReceiverNumber, number);
                iSent.PutExtra(OneSmsAction.SmsTransactionId, "482498");

                PendingIntent piSent = PendingIntent.GetBroadcast(Context, int.Parse(number), iSent, 0);

                Intent iDel = new Intent(OneSmsAction.SmsDelivered);
                iDel.PutExtra(OneSmsAction.ReceiverNumber, number);
                iDel.PutExtra(OneSmsAction.SmsTransactionId, "482498");
                PendingIntent piDel = PendingIntent.GetBroadcast(Context, 0, iDel, 0);

                if (Build.VERSION.SdkInt >= BuildVersionCodes.LollipopMr1)
                {
                    SubscriptionManager localSubscriptionManager = (SubscriptionManager)Context.GetSystemService(Context.TelephonySubscriptionService);

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

        public async Task SendSms(SmsRequest smsTransactionDto)
        {
            var smsPermission = await CheckAndRequestSmsPermission();
            var readPhoneStatePermission = await CheckAndRequestReadPhoneStatePermission();
            if (smsPermission == PermissionStatus.Granted && readPhoneStatePermission == PermissionStatus.Granted)
            {
                SmsManager sm = SmsManager.Default;
                List<string> parts = sm.DivideMessage(smsTransactionDto.Body).ToList();
                var bundle = new Bundle(1);
                bundle.PutInt(OneSmsAction.SmsTransactionId, smsTransactionDto.SmsId);
                Intent iSent = new Intent(OneSmsAction.SmsSent);
                iSent.PutExtra(OneSmsAction.SmsBundleId, bundle);
                iSent.PutExtra(OneSmsAction.ReceiverNumber, smsTransactionDto.ReceiverNumber);
                iSent.PutExtra(OneSmsAction.AppId, smsTransactionDto.AppId.ToString());
                iSent.PutExtra(OneSmsAction.SenderNumber, smsTransactionDto.SenderNumber);
                iSent.PutExtra(OneSmsAction.MobileServerId, smsTransactionDto.MobileServerId.ToString());

                PendingIntent piSent = PendingIntent.GetBroadcast(Context, smsTransactionDto.SmsId, iSent, PendingIntentFlags.UpdateCurrent);

                Intent iDel = new Intent(OneSmsAction.SmsDelivered);
                iDel.PutExtra(OneSmsAction.SmsBundleId, bundle);
                iDel.PutExtra(OneSmsAction.ReceiverNumber, smsTransactionDto.ReceiverNumber);
                iDel.PutExtra(OneSmsAction.AppId, smsTransactionDto.AppId.ToString());
                iDel.PutExtra(OneSmsAction.SenderNumber, smsTransactionDto.SenderNumber);
                iDel.PutExtra(OneSmsAction.MobileServerId, smsTransactionDto.MobileServerId.ToString());

                PendingIntent piDel = PendingIntent.GetBroadcast(Context, smsTransactionDto.SmsId, iDel, PendingIntentFlags.UpdateCurrent);

                if (Build.VERSION.SdkInt >= BuildVersionCodes.LollipopMr1)
                {
                    SubscriptionManager localSubscriptionManager = (SubscriptionManager)Context.GetSystemService(Context.TelephonySubscriptionService);

                    if (localSubscriptionManager.ActiveSubscriptionInfoCount > 1)
                    {
                        var localList = localSubscriptionManager.ActiveSubscriptionInfoList;

                        SubscriptionInfo simInfo1 = localList[0];
                        SubscriptionInfo simInfo2 = localList[1];
                        if (smsTransactionDto.SimSlot == 0)
                        {
                            //SendSMS From SIM One
                            SmsManager.GetSmsManagerForSubscriptionId(simInfo1.SubscriptionId).SendTextMessage(smsTransactionDto.ReceiverNumber, null, smsTransactionDto.Body, piSent, piDel);
                        }
                        else
                        {
                            //SendSMS From SIM Two
                            SmsManager.GetSmsManagerForSubscriptionId(simInfo2.SubscriptionId).SendTextMessage(smsTransactionDto.ReceiverNumber, null, smsTransactionDto.Body, piSent, piDel);
                        }
                    }
                }
                else
                {
                    if (parts.Count == 1)
                    {
                        smsTransactionDto.Body = parts[0];
                        sm.SendTextMessage(smsTransactionDto.ReceiverNumber, null, smsTransactionDto.Body, piSent, piDel);
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

        public Task<Result<string>> SendReceivedSms(SmsReceived smsReceived) => _httpClientService.PutAsync<string>(smsReceived, ApiRoutes.Sms.SmsReceived);

        public async Task<PermissionStatus> CheckAndRequestSmsPermission()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.Sms>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Sms>();
            }

            // Additionally could prompt the user to turn on in settings

            return status;
        }

        public async Task<PermissionStatus> CheckAndRequestReadPhoneStatePermission()
        {
            var status = await Permissions.CheckStatusAsync<ReadPhoneStatePermission>();
            if (status != PermissionStatus.Granted)
                status = await Permissions.RequestAsync<ReadPhoneStatePermission>();
            return status;
        }


    }
}