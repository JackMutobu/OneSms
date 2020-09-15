using Akavache;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Provider;
using Android.Widget;
using Java.Net;
using Microsoft.AspNetCore.SignalR.Client;
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
using OneSms.Droid.Server.Constants;
using OneSms.Contracts.V1.MobileServerRequest;
using OneSms.Contracts.V1.Enumerations;
using OneSms.Contracts.V1;
using Java.IO;

namespace OneSms.Droid.Server.Services
{
    public interface IWhatsappService
    {
        object CurrentTransaction { get; }
        Subject<object> OnRequestCompleted { get; }

        Task<PermissionStatus> CheckAndRequestReadContactPermission();
        Task<PermissionStatus> CheckAndRequestWriteContactPermission();

        Task CheckContactAndSendVcard(Context context, string vcard, string number, string message);
        Task Execute<T>(T request) where T : BaseMessageRequest;
        void SendText(string number, string message);
    }

    public class WhatsappService : IWhatsappService
    {
        private Queue<object> _transactionQueue;
        private ISignalRService _signalRService;
        private IHttpClientService _httpClientService;
        private bool _isBusy;
        private Context _context;

        public WhatsappService(Context context)
        {
            _context = context;
            _signalRService = Locator.Current.GetService<ISignalRService>();
            _httpClientService = Locator.Current.GetService<IHttpClientService>();
            OnRequestCompleted = new Subject<object>();
            _transactionQueue = new Queue<object>();
            _signalRService.Connection.On(SignalRKeys.ResetToActive, () => _isBusy = false);
            _signalRService.Connection.On<WhatsappRequest>(SignalRKeys.SendWhatsapp, async transaction => await Execute(transaction));
            _signalRService.Connection.On<ShareContactRequest>(SignalRKeys.ShareContact, async contact => await Execute(contact));


            OnRequestCompleted.Subscribe(async request =>
            {
                _isBusy = false;
                switch (request)
                {
                    case WhatsappRequest whatsappRequest:
                        await ReportRequestState(whatsappRequest);
                        break;
                }

                await ExecuteNext();

                async Task ExecuteNext()
                {
                    if (_transactionQueue.Count > 0)
                    {
                        var nextTransaction = _transactionQueue.Dequeue();
                        switch (nextTransaction)
                        {
                            case WhatsappRequest wRequest:
                                await Execute(wRequest);
                                break;
                            case ShareContactRequest sRequest:
                                await Execute(sRequest);
                                break;
                        }
                    }
                    else
                    {
                        CurrentTransaction = null;
                    }
                }

                async Task ReportRequestState(WhatsappRequest whatsappRequest)
                {
                    whatsappRequest.MessageStatus = await BlobCache.LocalMachine.GetObject<MessageStatus>(OneSmsAction.MessageStatus);
                    _httpClientService.PutAsync<string>(whatsappRequest, ApiRoutes.Whatsapp.StatusChanged);
                }
            });
        }

        public Subject<object> OnRequestCompleted { get; }


        public async Task Execute<T>(T request) where T : BaseMessageRequest
        {
            if (_isBusy)
                _transactionQueue.Enqueue(request);
            else
            {
                _isBusy = true;
                CurrentTransaction = request;
                if (request is WhatsappRequest wRequest)
                {
                    wRequest.MessageStatus = MessageStatus.Executing;
                    _httpClientService.PutAsync<string>(wRequest, ApiRoutes.Whatsapp.StatusChanged);
                    await SendAsync(wRequest);
                }
                else if (request is ShareContactRequest sRequest)
                {
                    await CheckContactAndSendVcard(_context, sRequest.VcardInfo, sRequest.ReceiverNumber, sRequest.Body);
                }
            }
        }

        public object CurrentTransaction { get; private set; }

        public void SendText(string number, string message)
        {
            var packageManager = _context.PackageManager;
            Intent i = new Intent(Intent.ActionView);

            try
            {
                var url = $"https://api.whatsapp.com/send?phone={number}&text={URLEncoder.Encode(message, "UTF-8")}";
                i.SetPackage("com.whatsapp.w4b");
                i.SetData(Uri.Parse(url));
                i.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
                if (i.ResolveActivity(packageManager) != null)
                    _context.StartActivity(i);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine($"{e.Message}\n { e.StackTrace}");
            }
        }

        private async Task SendAsync(WhatsappRequest transaction)
        {
            var imageLink = transaction.ImageLinks.Where(x => !string.IsNullOrEmpty(x)).FirstOrDefault();
            if (string.IsNullOrEmpty(imageLink))
                SendText(transaction.ReceiverNumber, transaction.Body);
            else
            {
                var imageBytes = await BlobCache.LocalMachine.DownloadUrl(imageLink);
                var image = await BitmapFactory.DecodeByteArrayAsync(imageBytes, 0, imageBytes.Length);
                await CheckContactAndSendImage(_context, image, transaction.ReceiverNumber, transaction.Body);
                Preferences.Set(OneSmsAction.ImageTransaction, transaction.MessageId);
            }
        }

        private async Task CheckContactAndSendImage(Context context, Bitmap bitmap, string number, string message)
        {
            var toNumber = number.Replace("+", "").Replace(" ", "");
            var contactId = await GetContactId(context, toNumber);
            if (!string.IsNullOrEmpty(contactId))
                SendImage(context, bitmap, toNumber, message);
            else
            {
                var contactCreated = await CreateContact(context, number.Replace(" ", ""));
                if (contactCreated)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));//wait for contact to be properly registered
                    await CheckContactAndSendImage(context, bitmap, number, message);
                }
                else
                    SendImage(context, bitmap, toNumber, message);
            }

        }

        private void SendImage(Context context, Bitmap bitmap, string number, string message)
        {
            var imgUri = GetLocalBitmapUri(context, bitmap);
            Intent i = new Intent(Intent.ActionSend);
            i.SetType("text/plain");
            i.SetPackage("com.whatsapp.w4b");
            i.PutExtra(Intent.ExtraText, message);
            i.PutExtra(Intent.ExtraStream, imgUri);
            i.PutExtra("jid", $"{number}@s.whatsapp.net");
            i.SetType("image/jpeg");
            i.AddFlags(ActivityFlags.GrantReadUriPermission | ActivityFlags.NewTask | ActivityFlags.ClearTask);

            try
            {
                _context.StartActivity(i);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                Toast.MakeText(_context, ex.Message, ToastLength.Long);
            }
        }

        public async Task CheckContactAndSendVcard(Context context, string vcard, string number, string message)
        {
            var toNumber = number.Replace("+", "").Replace(" ", "");
            var contactId = await GetContactId(context, toNumber);
            if (!string.IsNullOrEmpty(contactId))
                SendVcard(context, toNumber, message, vcard);
            else
            {
                var contactCreated = await CreateContact(context, number.Replace(" ", ""));
                if (contactCreated)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));//wait for contact to be registered
                    await CheckContactAndSendVcard(context, number, message, vcard);
                }
                else
                    SendVcard(context, toNumber, message, vcard);
            }

        }

        private void SendVcard(Context context, string number, string message, string vcard)
        {
            try
            {
                var vcardUri = GetLocalVcardUri(context, vcard);
                Intent i = new Intent(Intent.ActionSend);
                i.SetType("text/plain");
                i.SetPackage("com.whatsapp.w4b");
                i.PutExtra(Intent.ExtraText, message);
                i.PutExtra(Intent.ExtraStream, vcardUri);
                i.PutExtra("jid", $"{number}@s.whatsapp.net");
                i.SetType("*/*");
                i.AddFlags(ActivityFlags.GrantReadUriPermission | ActivityFlags.NewTask | ActivityFlags.ClearTask);
                _context.StartActivity(i);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine($"{e.Message}\n { e.StackTrace}");
            }
        }

        private async Task<string> GetContactId(Context context, string number)
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

        private async Task<bool> CreateContact(Context context, string number)
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

        private Uri GetLocalBitmapUri(Context context, Bitmap bitmap)
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

        private Uri GetLocalVcardUri(Context context, string vcardInfo)
        {
            try
            {
                string path = System.IO.Path.Combine(Xamarin.Essentials.FileSystem.CacheDirectory, "onesms_sahre" + DateTime.Now.Millisecond + ".vcf");
                using FileOutputStream outfile = new FileOutputStream(path);
                outfile.Write(System.Text.Encoding.ASCII.GetBytes(vcardInfo));
                outfile.Close();
                File file = new File(path);
                var vcardUri = GetUriFromFile(context, file);
                return vcardUri;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
            return null;
        }

        private Uri GetUriFromFile(Context context, File file)
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

        private async Task<string> GetWhatsappNumber(Context context, string contactId)
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


    }
}