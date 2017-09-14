using Android.App;
using Android.Widget;
using Android.OS;

using System.Collections.Generic;
using System;
using Android.Views;
using Android.Content;
using static Android.Widget.AdapterView;

using System.Linq;

namespace MusicPlayer
{
    [Activity(Label = "Music Player", MainLauncher = true)]
    public class MainActivity : Activity
    {

        private static bool startupCalled = false;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Main);

            //If the application was just launched.
            if (!startupCalled)
            {     
                //Calls the constructor of Initalize class.
                new AppUtil.OnInitalize(this);
                startupCalled = true;
            }

            ListView musicList = FindViewById<ListView>(Resource.Id.musicList);

            //Gets the list of song titles from Songdata.
            var songTitles = new SongData().GetList().Select(song => song.GetSongTitle());

            //Populates the listview with the list of song titles.
            musicList.Adapter = new ArrayAdapter<string>(this, Resource.Layout.List_Music, songTitles.ToList());

            musicList.TextFilterEnabled = true;
            //When the user selects a song in the list.
            musicList.ItemClick += OnSongSelected;
        }

        private void OnSongSelected(object sender, ItemClickEventArgs itemEventArgs) {
            string songTitle = ((TextView)itemEventArgs.View).Text;
            Intent intent = new Intent(this, typeof(PlayMusicActivity));

            //As we did not sort the list
            //the list's songID is synced with the listview's row position.

            //Sends the Songid to the next activity.
            intent.PutExtra("SONGID", itemEventArgs.Position);

            //Uses string instead of bool
            //as we can check for nulls instead of using default.
            intent.PutExtra("FAVORITE", "YES");
            StartActivity(intent);
        }
    }
}

