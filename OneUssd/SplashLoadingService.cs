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
    [Service(Enabled = true, Exported = false)]
    public class SplashLoadingService : Service
    {
        private LinearLayout layout;
        private IWindowManager wm;
        
        public override IBinder OnBind(Intent intent)
        {
            return null;
        }
        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            wm = Application.Context.GetSystemService(WindowService).JavaCast<IWindowManager>();
            Point size = new Point();
            wm.DefaultDisplay.GetSize(size);
            WindowManagerTypes layoutFlag;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                layoutFlag = WindowManagerTypes.ApplicationOverlay;
            else
                layoutFlag = WindowManagerTypes.Phone;

            int padding_in_dp = 100;
            var scale = Resources.DisplayMetrics.Density;
            int padding_in_px = (int)(padding_in_dp * scale + 0.5f);

            layout = new LinearLayout(this);
            layout.SetBackgroundColor(Color.White);
            layout.Orientation = Orientation.Vertical;

            var layoutParams = new WindowManagerLayoutParams(WindowManagerLayoutParams.MatchParent, WindowManagerLayoutParams.MatchParent, layoutFlag,
           WindowManagerFlags.NotFocusable | WindowManagerFlags.NotTouchModal,
           Format.Rgb565);
            LinearLayout.LayoutParams params_ll = new LinearLayout
                    .LayoutParams(ViewGroup.LayoutParams.MatchParent, 0);
            params_ll.Gravity = GravityFlags.Center;
            params_ll.Weight = 1;


            RelativeLayout relativeLayout = new RelativeLayout(this);
            RelativeLayout.LayoutParams rp = new RelativeLayout.LayoutParams(
                    WindowManagerLayoutParams.MatchParent,
                    WindowManagerLayoutParams.MatchParent);
            rp.AddRule(LayoutRules.CenterInParent);

            GifImageView gifImageView = new GifImageView(this);
            gifImageView.SetGifImageResource(Resource.Drawable.loading);
            gifImageView.SetPaddingRelative(0, padding_in_px, 0, padding_in_px);
            relativeLayout = new RelativeLayout(this);
            rp = new RelativeLayout.LayoutParams(
                    WindowManagerLayoutParams.MatchParent,
                    WindowManagerLayoutParams.MatchParent);
            relativeLayout.AddView(gifImageView, rp);
            layout.AddView(relativeLayout, params_ll);

            wm.AddView(layout, layoutParams);
            return StartCommandResult.Sticky;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            _ = new Handler().PostDelayed(() =>
            {
                if (layout != null)
                {
                    wm.RemoveView(layout);
                    layout = null;
                }
            }, 500);
        }
    }
}