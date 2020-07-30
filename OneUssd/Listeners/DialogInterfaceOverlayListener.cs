using Android.App;
using Android.Content;
using Android.Net;

namespace OneUssd.Listeners
{
    public class DialogInterfaceOverlayListener : Java.Lang.Object, IDialogInterfaceOnClickListener
    {
        private Activity _activity;
        public DialogInterfaceOverlayListener(Activity activity)
        {
            _activity = activity;
        }
        public void OnClick(IDialogInterface dialog, int which)
        {
            var intent = new Intent(Android.Provider.Settings.ActionManageOverlayPermission,Uri.Parse("package:" + _activity.PackageName));
            _activity.StartActivity(intent);
        }
    }
}