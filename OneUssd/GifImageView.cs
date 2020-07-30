using Android.Content;
using Android.Graphics;
using Android.Net;
using Android.OS;
using Android.Util;
using Android.Views;
using Java.IO;
using System.IO;

namespace OneUssd
{
    public class GifImageView : View
    {
        private Stream mInputStream;
        private Movie mMovie;
        private int mWidth, mHeight;
        private long mStart;
        private Context mContext;

        public GifImageView(Context context):base(context)
        {
            this.mContext = context;
        }

        public GifImageView(Context context, IAttributeSet attrs) :base(context, attrs)
        {
            Initialize();
        }

        public GifImageView(Context context, IAttributeSet attrs, int defStyle):base(context, attrs, defStyle)
        {
            Initialize();
            if (attrs.GetAttributeName(1).Equals("background"))
            {
                int id = int.Parse(attrs.GetAttributeValue(1).Substring(1));
                SetGifImageResource(id);
            }
        }

        private void Initialize()
        {
            SetFocusable(ViewFocusability.FocusableAuto);
            mMovie = Movie.DecodeStream(mInputStream);
            mWidth = mMovie.Width();
            mHeight = mMovie.Height();
            RequestLayout();
        }
        public void SetGifImageResource(int id)
        {
            mInputStream = mContext.Resources.OpenRawResource(id);
            Initialize();
        }
        public void SetGifImageUri(Uri uri)
        {
            try
            {
                mInputStream = mContext.ContentResolver.OpenInputStream(uri);
                Initialize();
            }
            catch (System.IO.FileNotFoundException e)
            {
                Log.Error("GIfImageView", "File not found");
            }
        }
        protected override void OnDraw(Canvas canvas)
        {
            long now = SystemClock.UptimeMillis();

            if (mStart == 0)
            {
                mStart = now;
            }

            if (mMovie != null)
            {

                int duration = mMovie.Duration();
                if (duration == 0)
                {
                    duration = 1000;
                }

                int relTime = (int)((now - mStart) % duration);

                mMovie.SetTime(relTime);

                mMovie.Draw(canvas, 0, 0);
                Invalidate();
            }
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
        }
    }
}