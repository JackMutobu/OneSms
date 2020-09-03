using Akavache;
using Android.App;
using Android.Content;
using OneSms.Contracts.V1.Requests;
using OneSms.Droid.Server.Constants;
using Splat;
using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace OneSms.Droid.Server.Services
{
    public interface IAuthService
    {
        Subject<bool> OnAuthStateChanged { get; }

        Task<bool> Authenticate();
    }

    public class AuthService : IAuthService
    {
        private readonly IHttpClientService _httpClientService;
        private readonly Context _context;

        public AuthService(Context context)
        {
            _httpClientService = Locator.Current.GetService<IHttpClientService>();
            _context = context;
            OnAuthStateChanged = new Subject<bool>();
        }

        public Subject<bool> OnAuthStateChanged { get; }

        public async Task<bool> Authenticate()
        {
            try
            {
                var authKey = await BlobCache.LocalMachine.GetObject<string>(OneSmsAction.AuthKey).Catch(Observable.Return(string.Empty));
                if (string.IsNullOrEmpty(authKey))
                {
                    var server = new ServerAuthRequest
                    {
                        ServerKey = Preferences.Get(OneSmsAction.ServerKey, string.Empty),
                        Secret = Preferences.Get(OneSmsAction.ServerSecret, string.Empty)
                    };
                    var authResponse = await _httpClientService.Authenticate(server);
                    authKey = authResponse.Token;
                    await BlobCache.LocalMachine.InsertObject(OneSmsAction.AuthKey, authKey, TimeSpan.FromDays(29));
                }
                _httpClientService.SetAuthorizationHeaderToken(authKey);
                OnAuthStateChanged.OnNext(true);
                return true;
            }
            catch (Exception ex)
            {
                AlertDialog alertDialog = new AlertDialog.Builder(_context).Create();
                alertDialog.SetTitle("Exception");
                alertDialog.SetMessage($"Message:{ex.Message}");
                alertDialog.Show();
            }
            OnAuthStateChanged.OnNext(false);
            return false;
        }
    }
}