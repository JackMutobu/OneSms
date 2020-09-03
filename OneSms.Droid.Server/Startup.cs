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

            Locator.CurrentMutable.RegisterConstant<ISignalRService>(new SignalRService(context,singalRBaseUrl));

            Locator.CurrentMutable.RegisterConstant<ISmsService>(new SmsService(context));

            Locator.CurrentMutable.RegisterConstant<IUssdService>(new UssdService(context));

            Locator.CurrentMutable.RegisterConstant<IWhatsappService>(new WhatsappService(context));

            Locator.CurrentMutable.RegisterConstant<IAuthService>(new AuthService(context));
        }
    }
}