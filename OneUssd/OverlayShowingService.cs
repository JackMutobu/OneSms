using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Threading.Tasks;

namespace OneUssd
{
    [Service(Enabled = true,Exported = false)]
    public class OverlayShowingService : Service
    {
        private Button overlayedButton;
        private IWindowManager wm;
        public const string EXTRA = "TITLE";
        private string title = null;
        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            if (intent.HasExtra(EXTRA))
                title = intent.GetStringExtra(EXTRA);
            wm = Application.Context.GetSystemService(WindowService).JavaCast<IWindowManager>();
            Point size = new Point();
            wm.DefaultDisplay.GetSize(size);
            WindowManagerTypes layoutFlag;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                layoutFlag = WindowManagerTypes.ApplicationOverlay;
            else
                layoutFlag = WindowManagerTypes.Phone;

            overlayedButton = new Button(this)
            {
                Text = title,
                Alpha = 0.7f
            };
            overlayedButton.SetBackgroundColor(Color.White);
            overlayedButton.TextSize = 26;
            var layoutParams = new WindowManagerLayoutParams(WindowManagerLayoutParams.MatchParent, size.Y - 200, layoutFlag,
           WindowManagerFlags.NotFocusable | WindowManagerFlags.NotTouchModal,
           Format.Translucent)
            {
                Gravity = GravityFlags.Center
            };
            wm.AddView(overlayedButton, layoutParams);
            return StartCommandResult.Sticky;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            _ = new Handler().PostDelayed(() =>
            {
                if (overlayedButton != null)
                {
                    wm.RemoveView(overlayedButton);
                    overlayedButton = null;
                }
            }, 500);
        }
    }
}