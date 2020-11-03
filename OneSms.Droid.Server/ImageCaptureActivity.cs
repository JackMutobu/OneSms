
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Hardware.Display;
using Android.Media;
using Android.Media.Projection;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using OneSms.Droid.Server.Constants;
using OneSms.Droid.Server.Extensions;
using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace OneSms.Droid.Server
{
    [Activity(Label = "ImageCaptureActivity",Theme = "@style/TransparentTheme",LaunchMode = LaunchMode.SingleTop)]
    public class ImageCaptureActivity : Activity
    {
        private const int RequestMediaProjection = 1;
        private MediaProjectionManager _mediaProjectionManager;
        private MediaProjection _mediaProjection = null;
        private ImageReader _imageReader;
        private const int _maxImageBuffer = 10;
        private bool _keepRunning;

        public VirtualDisplay VirtualDisplay { get; private set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_image_capture);

            _keepRunning = Intent.GetBooleanExtra(OneSmsAction.KeepRunning, true);
            if(_keepRunning)
                StartCapturing();

        }

        protected async override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            if (requestCode == RequestMediaProjection)
            {
                if (resultCode != Result.Ok)
                {
                    _mediaProjection = null;
                    Toast toast = Toast.MakeText(this, "Media Projection Declined", ToastLength.Short);
                    toast.Show();
                }
                else
                {
                    _mediaProjection = _mediaProjectionManager.GetMediaProjection((int)resultCode, data);
                    await AttachImageCaptureOverlay();
                }

                Finish();

            }
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
            _keepRunning = intent.GetBooleanExtra(OneSmsAction.KeepRunning, false);
            if (_keepRunning == false)
                this.Finish();
            else
                StartCapturing();
        }

        private void StartCapturing()
        {
            _mediaProjectionManager = (MediaProjectionManager)GetSystemService(MediaProjectionService);
            StartActivityForResult(_mediaProjectionManager.CreateScreenCaptureIntent(), RequestMediaProjection);
        }

        public async Task AttachImageCaptureOverlay()
        {

            if (_mediaProjection == null) return;

            DisplayMetrics metrics = new DisplayMetrics();
            this.WindowManager.DefaultDisplay.GetRealMetrics(metrics);
            _imageReader = ImageReaderEx.NewInstance(metrics.WidthPixels, metrics.HeightPixels,Format.Rgba8888, _maxImageBuffer);

             VirtualDisplay = _mediaProjection.CreateVirtualDisplay("ScreenCaptureTest",
                    metrics.WidthPixels, metrics.HeightPixels, (int)metrics.DensityDpi, DisplayFlags.Presentation,
                    _imageReader.Surface, null, null);


            await OnImageAvailable(_imageReader);
        }

        public void DetachImageCaptureOverlay()
        {
            VirtualDisplay.Release();
            _imageReader.Close();
        }

        public async Task  OnImageAvailable(ImageReader reader)
        {
            await Task.Delay(500);//Wait for the screenshot to be acquired
            Image image = reader.AcquireLatestImage();

            if (image == null || image.GetPlanes().Length <= 0) return;

            Image.Plane plane = image.GetPlanes()[0];

            int rowPadding = plane.RowStride - plane.PixelStride * image.Width;
            int bitmapWidth = image.Width + rowPadding / plane.PixelStride;

            var tempBitmap = Bitmap.CreateBitmap(bitmapWidth, image.Height, Bitmap.Config.Argb8888);
            tempBitmap.CopyPixelsFromBuffer(plane.Buffer);

            Rect cropRect = image.CropRect;
            Bitmap bitmap = Bitmap.CreateBitmap(tempBitmap, cropRect.Left, cropRect.Top, cropRect.Width(), cropRect.Height());
            
            if(await CheckAndRequestStoragePermission() == PermissionStatus.Granted)
                SaveImage(bitmap);

            image.Close();
        }

        public void SaveImage(Bitmap image)
        {
            try
            {
                var jFolder = new Java.IO.File(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures),"");
                if (!jFolder.Exists())
                    jFolder.Mkdirs();

                var jFile = new Java.IO.File(jFolder, $"IMG-{DateTime.UtcNow.Day}-{DateTime.UtcNow.Second}.png");

                // Save File
                using var fs = new FileStream(jFile.AbsolutePath, FileMode.CreateNew);
                image.Compress(Bitmap.CompressFormat.Png, 100, fs);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private async Task<PermissionStatus> CheckAndRequestStoragePermission()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
            if (status != PermissionStatus.Granted)
                status = await Permissions.RequestAsync<Permissions.StorageWrite>();
            return status;
        }

        public static void CheckScreenRecorderPermissions(Context context)
        {
            var mediaProjectionManager = (MediaProjectionManager)context.GetSystemService(MediaProjectionService);
            context.StartActivity(mediaProjectionManager.CreateScreenCaptureIntent());
        }
    }
    public class OrientationChangedListener : OrientationEventListener
    {

        int _lastOrientation = -1;
        private Context _context;
        private IWindowManager _windowManager;
        private VirtualDisplay _virtualDisplay;
        private ImageCaptureActivity _imageCaptureActivity;

        public OrientationChangedListener(Context context, ImageCaptureActivity imageCaptureActivity) : base(context)
        {
            _context = context;
            _windowManager = imageCaptureActivity.WindowManager;
            _virtualDisplay = imageCaptureActivity.VirtualDisplay;
            _imageCaptureActivity = imageCaptureActivity;
        }

        public async override void OnOrientationChanged(int orientation)
        {
            var screenOrientation = _windowManager.DefaultDisplay.Rotation;

            if (_virtualDisplay == null) return;

            if (_lastOrientation == (int)screenOrientation) return;

            _lastOrientation = (int)screenOrientation;

            _imageCaptureActivity.DetachImageCaptureOverlay();
            await _imageCaptureActivity.AttachImageCaptureOverlay();
        }
    }

}