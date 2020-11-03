using Android.Media;
using Android.Graphics;
using static Android.Media.ImageReader;
using System.IO;
using System;

namespace OneSms.Droid.Server.Listners
{
    public class ImageAvailableListener : Java.Lang.Object, IOnImageAvailableListener
    {
        public void OnImageAvailable(ImageReader reader)
        {
            Image image = reader.AcquireLatestImage();

            if (image == null || image.GetPlanes().Length <= 0) return;

            Image.Plane plane = image.GetPlanes()[0];

            int rowPadding = plane.RowStride - plane.PixelStride * image.Width;
            int bitmapWidth = image.Width + rowPadding / plane.PixelStride;

            var tempBitmap = Bitmap.CreateBitmap(bitmapWidth, image.Height, Bitmap.Config.Argb8888);
            tempBitmap.CopyPixelsFromBuffer(plane.Buffer);

            Rect cropRect = image.CropRect;
            Bitmap bitmap = Bitmap.CreateBitmap(tempBitmap, cropRect.Left, cropRect.Top, cropRect.Width(), cropRect.Height());

            SaveImage(bitmap);

            image.Close();
        }

        public void SaveImage(Bitmap image)
        {
            try
            {
                var jFolder = new Java.IO.File(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures), "OneSms");
                if (!jFolder.Exists())
                    jFolder.Mkdirs();

                var jFile = new Java.IO.File(jFolder, "OneSms.jpg");

                // Save File
                using (var fs = new FileStream(jFile.AbsolutePath, FileMode.CreateNew))
                {
                    image.Compress(Bitmap.CompressFormat.Png, 100, fs);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }


}