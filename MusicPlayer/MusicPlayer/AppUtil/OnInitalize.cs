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
using Android.Content.Res;
using Android.Database;

using Android.Graphics;
using System.IO;
using Android.Provider;

namespace MusicPlayer.AppUtil
{
    //Called once when the app starts
    public class OnInitalize
    {
        public OnInitalize(Context context) {
            PopulateSongList(context);
        }
        
        public void PopulateSongList(Context context) {
            SongData songData = new SongData();
            Android.Net.Uri musicUri = MediaStore.Audio.Media.ExternalContentUri;
            //Used to query the media files.
            ICursor musicCursor = context.ContentResolver.Query(musicUri, null, null, null, null);

            if (musicCursor != null && musicCursor.MoveToFirst())
            {
                int songID = -1;

                //get columns.
                int fileIsMusicColumn = musicCursor.GetColumnIndex("is_music");
                int songTitleColumn = musicCursor.GetColumnIndex("title");
                int songArtistColumn = musicCursor.GetColumnIndex("artist");
                int albumIDColumn = musicCursor.GetColumnIndex("album_id");
                int fileURIColumn = musicCursor.GetColumnIndex("_data");
                //There is also a is_favorite column.
                //But I am not using it for this example.

                //musicCursor.GetColumnNames();

                //add songs to the songData list
                do
                {

                    //If the current file is a music file.
                    if (musicCursor.GetInt(fileIsMusicColumn) == 1)
                    {
                        string songTitle = musicCursor.GetString(songTitleColumn);
                        string songArtist = musicCursor.GetString(songArtistColumn);
                        Android.Net.Uri fileDir = Android.Net.Uri.Parse(musicCursor.GetString(fileURIColumn));
                        var test = musicCursor.GetString(fileURIColumn);
                        //We will use this to fetch the song image later.
                        //Store a list of Bitmaps will take up too much space.
                        long albumID = musicCursor.GetLong(albumIDColumn);

                        //If song artist value is not found.
                        if (songArtist == null)
                        {
                            songArtist = "<unknown>";
                        }

                        songData.AddNewSong(new Song(++songID, songTitle, songArtist, albumID, fileDir));
                    }
                }
                while (musicCursor.MoveToNext());
            }
        }
    }
}