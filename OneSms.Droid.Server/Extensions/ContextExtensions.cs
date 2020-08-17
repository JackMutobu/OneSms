using Android.Content;
using OneSms.Droid.Server.Constants;
using System.Runtime.CompilerServices;
using Xamarin.Essentials;

namespace OneSms.Droid.Server.Extensions
{

    public static class ContextExtensions
    {
        public static void SetServiceState(this Context context, string serviceState)
        {
            Preferences.Set(OneSmsAction.ForegroundServiceState, serviceState);
        }
        public static string GetServiceState(this Context context) => Preferences.Get(OneSmsAction.ForegroundServiceState, OneSmsAction.ServiceStopped);
    }
}