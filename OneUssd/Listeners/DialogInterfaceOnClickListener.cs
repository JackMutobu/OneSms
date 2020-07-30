using Android.App;
using Android.Content;

namespace OneUssd.Listeners
{
    public class DialogInterfaceOnClickListener : Java.Lang.Object, IDialogInterfaceOnClickListener
    {
        private Activity _activity;

        public DialogInterfaceOnClickListener(Activity activity)
        {
            _activity = activity;
        }
        public void OnClick(IDialogInterface dialog, int which)
        {
            _activity.StartActivityForResult(new Intent(Android.Provider.Settings.ActionAccessibilitySettings), 1);
        }
    }
}