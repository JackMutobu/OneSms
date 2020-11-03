using Android.Media;
using Android.Runtime;
using Java.Interop;
using System.Linq;

namespace OneSms.Droid.Server.Extensions
{
    public static class ImageReaderEx
    {
        public unsafe static ImageReader NewInstance(int width, int height, Android.Graphics.Format format, int maxImages)
        {
            JniArgumentValue* ptr = stackalloc JniArgumentValue[4];
            *ptr = new JniArgumentValue(width);
            ptr[1] = new JniArgumentValue(height);
            ptr[2] = new JniArgumentValue((int)format);
            ptr[3] = new JniArgumentValue(maxImages);
            JniPeerMembers _members = new XAPeerMembers("android/media/ImageReader", typeof(ImageReader));
            return Java.Lang.Object.GetObject<ImageReader>(_members.StaticMethods.InvokeObjectMethod("newInstance.(IIII)Landroid/media/ImageReader;", ptr).Handle, JniHandleOwnership.TransferLocalRef);
        }

        public unsafe static ImageReader NewInstance(int width, int height, int format, int maxImages)
        {
            JniArgumentValue* ptr = stackalloc JniArgumentValue[4];
            *ptr = new JniArgumentValue(width);
            ptr[1] = new JniArgumentValue(height);
            ptr[2] = new JniArgumentValue(format);
            ptr[3] = new JniArgumentValue(maxImages);
            JniPeerMembers _members = new XAPeerMembers("android/media/ImageReader", typeof(ImageReader));
            return Java.Lang.Object.GetObject<ImageReader>(_members.StaticMethods.InvokeObjectMethod("newInstance.(IIII)Landroid/media/ImageReader;", ptr).Handle, JniHandleOwnership.TransferLocalRef);
        }

        public static int GetSize(this string value)
        {
            value = value.Replace(" ", "").Replace("KB", "").Replace("kB", "").Replace("\"", "");
            if (value.Contains("."))
                value = value.Substring(0, value.IndexOf('.'));
            return ConvertToInt(value);
        }

        public static int ConvertToInt(string value)
        {
            string number = string.Empty;
            var charNumbers = value.ToArray().Where(x => int.TryParse(x.ToString(), out int n)).ToList();
            charNumbers.ForEach(x => number += x);
            return int.Parse(number);
        }
    }
}