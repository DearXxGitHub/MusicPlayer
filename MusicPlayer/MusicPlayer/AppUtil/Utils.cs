using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using Android.Graphics;

namespace MusicPlayer.AppUtil
{
    public static class Utils
    {
        //Toast is used to display a mini-popup message only.
        private static Toast toast = null;

        //Dispose the previous toast if there is one.
        private static void ClearToast() {
            if (toast != null) {
                toast.Cancel();
                toast.Dispose();
                toast = null;
            }
        }
        //NOTE: If a toast is showing and you create a new toast, the toasts will be queued
        //unless you clear the currently showing toast.

        //Creates a new toast message.
        public static void ShowToast(Context context, string message, bool queueToast = false, ToastLength toastLength = ToastLength.Short) {
            //If the toast is not to be queued.
            if (!queueToast)
            {
                //Clears the currently showing toast if there is one.
                ClearToast();
            }
            //A Short toastLength will only show the Toast for a short period of time.

            toast = Toast.MakeText(context, message, toastLength);
            toast.Show();
        }

        //Used to cut down a string's length.   
        public static string Truncate(string value, int maxLength)
        {
            //If given string value was empty or null.
            if (string.IsNullOrEmpty(value)) { 
                //Simply returns back the value.
                return value; 
            }
            else
            {
                //If the string value is less than the desired max length then returns the given value.
                //Else cuts down the string length to the desired max length.
                return ((value.Length <= maxLength) ? value : value.Substring(0, maxLength));
            }
        }

        //Gets the album coverart for a music.
        public static Bitmap GetAlbumArt(long album_id, Context context)
        {
            Bitmap bm = null;
            try
            {
                Android.Net.Uri sArtworkUri = Android.Net.Uri.Parse("content://media/external/audio/albumart");

                Android.Net.Uri uri = ContentUris.WithAppendedId(sArtworkUri, album_id);

                ParcelFileDescriptor pfd = context.ContentResolver.OpenFileDescriptor(uri, "r");

                if (pfd != null)
                {
                    Java.IO.FileDescriptor fd = pfd.FileDescriptor;
                    bm = BitmapFactory.DecodeFileDescriptor(fd);
                }
            }
            catch (Exception e)
            {
            }
                //Returns null if there was a problem finding coverArt.
                return bm;
        }
    }
}