using Android.Content;
using Android.Database;
using Android.Net;
using Android.OS;
using Android.Provider;

namespace OneSms.Droid.Server.Extensions
{
    public static class UriExtensions
    {
        /// <summary>
        /// Method to return File path of a Gallery Image from URI.
        /// </summary>
        /// <param name="context">The Context.</param>
        /// <param name="uri">URI to Convert from.</param>
        /// <returns>The Full File Path.</returns>
        public static string GetPathFromUri(this Context context, Uri uri)
        {

            //check here to KITKAT or new version
            // bool isKitKat = Build.VERSION.SdkInt >= Build.VERSION_CODES.Kitkat;
            bool isKitKat = Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat;

            // DocumentProvider
            if (isKitKat && DocumentsContract.IsDocumentUri(context, uri))
            {

                // ExternalStorageProvider
                if (IsExternalStorageDocument(uri))
                {
                    string docId = DocumentsContract.GetDocumentId(uri);
                    string[] split = docId.Split(':');
                    string type = split[0];

                    if (type.Equals("primary", System.StringComparison.InvariantCultureIgnoreCase))
                    {
                        return Environment.ExternalStorageDirectory + "/" + split[1];
                    }
                }
                // DownloadsProvider
                else if (IsDownloadsDocument(uri))
                {

                    string id = DocumentsContract.GetDocumentId(uri);
                    Uri ContentUri = ContentUris.WithAppendedId(
                      Uri.Parse("content://downloads/public_downloads"), long.Parse(id));

                    return GetDataColumn(context, ContentUri, null, null);
                }
                // MediaProvider
                else if (IsMediaDocument(uri))
                {
                    string docId = DocumentsContract.GetDocumentId(uri);
                    string[] split = docId.Split(':');
                    string type = split[0];

                    Uri contentUri = null;
                    if ("image".Equals(type))
                    {
                        contentUri = MediaStore.Images.Media.ExternalContentUri;
                    }
                    else if ("video".Equals(type))
                    {
                        contentUri = MediaStore.Video.Media.ExternalContentUri;
                    }
                    else if ("audio".Equals(type))
                    {
                        contentUri = MediaStore.Audio.Media.ExternalContentUri;
                    }

                    string selection = "_id=?";
                    string[] selectionArgs = new string[] { split[1] };

                    return GetDataColumn(context, contentUri, selection, selectionArgs);
                }
            }
            // MediaStore (and general)
            else if (uri.Scheme.Equals("content", System.StringComparison.InvariantCultureIgnoreCase))
            {

                // Return the remote address
                if (IsGooglePhotosUri(uri))
                    return uri.LastPathSegment;

                return GetDataColumn(context, uri, null, null);
            }
            // File
            else if (uri.Scheme.Equals("file", System.StringComparison.InvariantCultureIgnoreCase))
            {
                return uri.Path;
            }

            return null;
        }

        /// <summary>
        /// Get the value of the data column for this URI. This is useful for
        /// MediaStore URIs, and other file-based ContentProviders.
        /// </summary>
        /// <param name="context">The Context.</param>
        /// <param name="uri">URI to Query</param>
        /// <param name="selection">(Optional) Filter used in the Query.</param>
        /// <param name="selectionArgs">(Optional) Selection Arguments used in the Query.</param>
        /// <returns>The value of the _data column, which is typically a File Path.</returns>
        private static string GetDataColumn(Context context, Uri uri, string selection, string[] selectionArgs)
        {
            ICursor cursor = null;
            string column = "_data";
            string[] projection = {
                    column
                };

            try
            {
                cursor = context.ContentResolver.Query(uri, projection, selection, selectionArgs,
                  null);
                if (cursor != null && cursor.MoveToFirst())
                {
                    int index = cursor.GetColumnIndexOrThrow(column);
                    return cursor.GetString(index);
                }
            }
            finally
            {
                if (cursor != null)
                    cursor.Close();
            }
            return null;
        }

        /// <param name="uri">The URI to Check.</param>
        /// <returns>Whether the URI Authority is ExternalStorageProvider.</returns>
        private static bool IsExternalStorageDocument(Uri uri)
        {
            return "com.android.externalstorage.documents".Equals(uri.Authority);
        }


        /// <param name="uri">The URI to Check.</param>
        /// <returns>Whether the URI Authority is DownloadsProvider.</returns>
        private static bool IsDownloadsDocument(Uri uri)
        {
            return "com.android.providers.downloads.documents".Equals(uri.Authority);
        }


        /// <param name="uri">The URI to Check.</param>
        /// <returns>Whether the URI Authority is MediaProvider.</returns>
        private static bool IsMediaDocument(Uri uri)
        {
            return "com.android.providers.media.documents".Equals(uri.Authority);
        }


        /// <param name="uri">The URI to check.</param>
        /// <returns>Whether the URI Authority is Google Photos.</returns>
        private static bool IsGooglePhotosUri(Uri uri)
        {
            return "com.google.android.apps.photos.content".Equals(uri.Authority);
        }
    }
}