using Android.Content;
using OneSms.Droid.Server.Services;
using Splat;

namespace OneSms.Droid.Server
{
    public class Startup
    {
        public static void Initialize(Context context,string httpBaseUrl, string singalRBaseUrl)
        {

            Locator.CurrentMutable.RegisterConstant<IHttpClientService>(new HttpClientService(httpBaseUrl));

            Locator.CurrentMutable.RegisterConstant<IAuthService>(new AuthService(context));

            Locator.CurrentMutable.RegisterConstant<ISignalRService>(new SignalRService(context,singalRBaseUrl));

            var whatsappService = new WhatsappService(context);
            var ussdService = new UssdService(context, whatsappService);
            var smsService = new SmsService(context, ussdService);
            whatsappService.Initialize(ussdService);

            Locator.CurrentMutable.RegisterConstant<ISmsService>(smsService);

            Locator.CurrentMutable.RegisterConstant<IWhatsappService>(whatsappService);

            Locator.CurrentMutable.RegisterConstant<IUssdService>(ussdService);
        }
    }
}