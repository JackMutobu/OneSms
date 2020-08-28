using Akavache;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Provider;
using Android.Widget;
using Java.Net;
using Microsoft.AspNetCore.SignalR.Client;
using OneSms.Web.Shared.Constants;
using OneSms.Web.Shared.Dtos;
using OneSms.Web.Shared.Enumerations;
using OneSms.Web.Shared.Models;
using Splat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Xamarin.Essentials;
using File = Java.IO.File;
using Uri = Android.Net.Uri;
using System.Reactive.Linq;
using Akavache.Core;
using OneSms.Droid.Server.Constants;

namespace OneSms.Droid.Server.Services
{
    public interface IWhatsappService
    {
        Context Context { get; set; }
        MessageTransactionProcessDto CurrentTransaction { get; }
        Subject<MessageTransactionProcessDto> OnMessageSent { get; }

        Task<PermissionStatus> CheckAndRequestReadContactPermission();
        Task<PermissionStatus> CheckAndRequestWriteContactPermission();
        Task CheckContactAndSendImage(Context context, Bitmap bitmap, string number, string message);
        Task<bool> CreateContact(Context context, string number);
        Task<string> GetContactId(Context context, string number);
        Uri GetLocalBitmapUri(Context context, Bitmap bitmap);
        Uri GetUriFromFile(Context context, File file);
        Task<string> GetWhatsappNumber(Context context, string contactId);
        Task SendAsync(MessageTransactionProcessDto transaction);
        void SendImage(Context context, Bitmap bitmap, string number, string message);
        void SendText(string number, string message);
    }

    public class WhatsappService : IWhatsappService
    {
        private Queue<MessageTransactionProcessDto> _transactionQueue;
        private ISignalRService _signalRService;
        private IHttpClientService _httpClientService;
        private bool _isBusy;
        public WhatsappService(Context context)
        {
            Context = context;
            _signalRService = Locator.Current.GetService<ISignalRService>();
            _httpClientService = Locator.Current.GetService<IHttpClientService>();
            OnMessageSent = new Subject<MessageTransactionProcessDto>();
            _transactionQueue = new Queue<MessageTransactionProcessDto>();
            _signalRService.Connection.On(SignalRKeys.SendWhatsapp, (Action<MessageTransactionProcessDto>)(async transaction => await Execute(transaction)));

            OnMessageSent.Subscribe(async transaction =>
            {
                _isBusy = false;
                if (_transactionQueue.Count > 0)
                {
                    CurrentTransaction = _transactionQueue.Dequeue();
                    await Execute(CurrentTransaction);
                }
                else
                    CurrentTransaction = null;
                transaction.TimeStamp = DateTime.UtcNow;
                transaction.TransactionState = MessageTransactionState.Sent;
                _httpClientService.PutAsync<string>(transaction, "Transaction/StatusChanged");
            });
        }

        public Context Context { get; set; }

        public MessageTransactionProcessDto CurrentTransaction { get; private set; }

        public Subject<MessageTransactionProcessDto> OnMessageSent { get; }

        private async Task Execute(MessageTransactionProcessDto transaction)
        {
            if (_isBusy)
                _transactionQueue.Enqueue(transaction);
            else
            {
                _isBusy = true;
                await SendAsync(transaction);
            }
        }

        public async Task SendAsync(MessageTransactionProcessDto transaction)
        {
            CurrentTransaction = transaction;
            var imageLink = transaction.ImageLinks.Where(x => !string.IsNullOrEmpty(x)).FirstOrDefault();
            if (string.IsNullOrEmpty(imageLink))
                  SendText(transaction.ReceiverNumber, transaction.Message);
            else
            {
                var imageBytes = await BlobCache.LocalMachine.DownloadUrl(imageLink);
                var image = await BitmapFactory.DecodeByteArrayAsync(imageBytes,0,imageBytes.Length);
                await CheckContactAndSendImage(Context, image, transaction.ReceiverNumber, transaction.Message);
                Preferences.Set(OneSmsAction.ImageTransaction, transaction.WhatsappId);
            }
        }

        public void SendText(string number, string message)
        {
            var packageManager = Context.PackageManager;
            Intent i = new Intent(Intent.ActionView);

            try
            {
                var url = $"https://api.whatsapp.com/send?phone={number}&text={URLEncoder.Encode(message, "UTF-8")}";
                i.SetPackage("com.whatsapp.w4b");
                i.SetData(Uri.Parse(url));
                i.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
                if (i.ResolveActivity(packageManager) != null)
                    Context.StartActivity(i);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine($"{e.Message}\n { e.StackTrace}");
            }
        }

        public async Task CheckContactAndSendImage(Context context, Bitmap bitmap, string number, string message)
        {
            var toNumber = number.Replace("+", "").Replace(" ", "");
            var contactId = await GetContactId(context, toNumber);
            if (!string.IsNullOrEmpty(contactId))
            {
                var whatsappNumber = await GetWhatsappNumber(context, contactId);
                if (string.IsNullOrEmpty(whatsappNumber))
                {
                    //Report number not on whatsapp, send sms
                }
                SendImage(context, bitmap, toNumber, message);
            }
            else
            {
                var contactCreated = await CreateContact(context, number.Replace(" ", ""));
                if (contactCreated)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    await CheckContactAndSendImage(context, bitmap, number, message);
                }
                else
                {
                    //Send sms
                }
            }

        }

        public void SendImage(Context context, Bitmap bitmap, string number, string message)
        {
            var imgUri = GetLocalBitmapUri(context, bitmap);
            Intent whatsappIntent = new Intent(Intent.ActionSend);
            whatsappIntent.SetType("text/plain");
            whatsappIntent.SetPackage("com.whatsapp.w4b");
            whatsappIntent.PutExtra(Intent.ExtraText, message);
            whatsappIntent.PutExtra(Intent.ExtraStream, imgUri);
            whatsappIntent.PutExtra("jid", $"{number}@s.whatsapp.net");
            whatsappIntent.SetType("image/jpeg");
            whatsappIntent.AddFlags(ActivityFlags.GrantReadUriPermission | ActivityFlags.NewTask | ActivityFlags.ClearTask);

            try
            {
                Context.StartActivity(whatsappIntent);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                Toast.MakeText(Context, ex.Message, ToastLength.Long);
            }
        }

        public async Task<string> GetContactId(Context context, string number)
        {
            if (await CheckAndRequestReadContactPermission() == PermissionStatus.Granted)
            {
                var contactId = string.Empty;
                if (!string.IsNullOrEmpty(number))
                {
                    var uri = Uri.WithAppendedPath(ContactsContract.PhoneLookup.ContentFilterUri, Uri.Encode(number));
                    var cursor = context.ContentResolver.Query(uri, new string[] { ContactsContract.PhoneLookup.InterfaceConsts.Id }, null, null);
                    if (cursor != null)
                    {
                        while (cursor.MoveToNext())
                            contactId = cursor.GetString(cursor.GetColumnIndexOrThrow(ContactsContract.PhoneLookup.InterfaceConsts.Id));
                        cursor.Close();
                    }
                }
                return contactId;
            }
            return string.Empty;
        }

        public async Task<string> GetWhatsappNumber(Context context, string contactId)
        {
            if (await CheckAndRequestReadContactPermission() == PermissionStatus.Granted)
            {
                var rowContactId = string.Empty;
                var projection = new string[] { ContactsContract.PhoneLookup.InterfaceConsts.Id };
                var selection = $"{ContactsContract.RawContacts.InterfaceConsts.ContactId} = ? AND {ContactsContract.RawContacts.InterfaceConsts.AccountType} = ?";
                var selectionArgs = new string[] { contactId, "com.whatsapp" };
                var cursor = context.ContentResolver.Query(ContactsContract.RawContacts.ContentUri, projection, selection, selectionArgs, null);
                if (cursor != null)
                {
                    bool hasWhatsapp = cursor.MoveToNext();
                    if (hasWhatsapp)
                        rowContactId = cursor.GetString(0);
                    cursor.Close();
                }
                return rowContactId;
            }
            return string.Empty;
        }

        public async Task<bool> CreateContact(Context context, string number)
        {
            if (await CheckAndRequestWriteContactPermission() == PermissionStatus.Granted)
            {
                try
                {
                    var ops = new List<ContentProviderOperation>();
                    int rawContactInsertIndex = ops.Count;

                    ContentProviderOperation.Builder builder =
                        ContentProviderOperation.NewInsert(ContactsContract.RawContacts.ContentUri);
                    builder.WithValue(ContactsContract.RawContacts.InterfaceConsts.AccountType, null);
                    builder.WithValue(ContactsContract.RawContacts.InterfaceConsts.AccountName, null);
                    ops.Add(builder.Build());

                    //Name
                    builder = ContentProviderOperation.NewInsert(ContactsContract.Data.ContentUri);
                    builder.WithValueBackReference(ContactsContract.Data.InterfaceConsts.RawContactId, rawContactInsertIndex);
                    builder.WithValue(ContactsContract.Data.InterfaceConsts.Mimetype,
                        ContactsContract.CommonDataKinds.StructuredName.ContentItemType);
                    builder.WithValue(ContactsContract.CommonDataKinds.StructuredName.FamilyName, "- OneSms");
                    builder.WithValue(ContactsContract.CommonDataKinds.StructuredName.GivenName, number);
                    ops.Add(builder.Build());

                    //Number
                    builder = ContentProviderOperation.NewInsert(ContactsContract.Data.ContentUri);
                    builder.WithValueBackReference(ContactsContract.Data.InterfaceConsts.RawContactId, rawContactInsertIndex);
                    builder.WithValue(ContactsContract.Data.InterfaceConsts.Mimetype,
                        ContactsContract.CommonDataKinds.Phone.ContentItemType);
                    builder.WithValue(ContactsContract.CommonDataKinds.Phone.Number, number);
                    builder.WithValue(ContactsContract.CommonDataKinds.StructuredPostal.InterfaceConsts.Type,
                            ContactsContract.CommonDataKinds.StructuredPostal.InterfaceConsts.TypeCustom);
                    builder.WithValue(ContactsContract.CommonDataKinds.Phone.InterfaceConsts.Label, "Mobile");
                    ops.Add(builder.Build());
                    var res = context.ContentResolver.ApplyBatch(ContactsContract.Authority, ops);
                    return true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                }
            }
            return false;
        }

        public Uri GetLocalBitmapUri(Context context, Bitmap bitmap)
        {

            Uri bmpUri = null;
            try
            {
                string path = System.IO.Path.Combine(Xamarin.Essentials.FileSystem.CacheDirectory, "onesms_sahre" + DateTime.Now.Millisecond + ".png");
                var fs = new FileStream(path, FileMode.OpenOrCreate);
                if (fs != null)
                {
                    bitmap.Compress(Bitmap.CompressFormat.Png, 90, fs);
                    fs.Close();
                }
                File file = new File(path);
                bmpUri = GetUriFromFile(context, file);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
            }
            return bmpUri;
        }

        public Uri GetUriFromFile(Context context, File file)
        {
            if (file == null)
                return null;

            if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
            {
                try
                {
                    return FileProvider.GetUriForFile(context.ApplicationContext, $"{context.PackageName}.fileprovider", file);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e);
                    return null;
                }
            }
            else
            {
                return Uri.FromFile(file);
            }
        }

        public async Task<PermissionStatus> CheckAndRequestReadContactPermission()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.ContactsRead>();
            if (status != PermissionStatus.Granted)
                status = await Permissions.RequestAsync<Permissions.ContactsRead>();
            return status;
        }

        public async Task<PermissionStatus> CheckAndRequestWriteContactPermission()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.ContactsWrite>();
            if (status != PermissionStatus.Granted)
                status = await Permissions.RequestAsync<Permissions.ContactsWrite>();
            return status;
        }

       
    }
}