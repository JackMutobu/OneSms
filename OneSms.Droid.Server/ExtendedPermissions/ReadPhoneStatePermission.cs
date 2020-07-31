using System.Collections.Generic;

namespace OneSms.Droid.Server.ExtendedPermissions
{
    public class ReadPhoneStatePermission : Xamarin.Essentials.Permissions.BasePlatformPermission
    {
        public override (string androidPermission, bool isRuntime)[] RequiredPermissions => new List<(string androidPermission, bool isRuntime)>
        {
            (Android.Manifest.Permission.ReadPhoneState, true),
        }.ToArray();
    }
}