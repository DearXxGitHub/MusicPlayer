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

namespace MusicPlayer
{
    //Used to store list of song data.
    public class SongData
    {
        //Creates a list to store the song data.
        private static List<Song> songList = new List<Song>();

        public void AddNewSong(Song song)
        {
            songList.Add(song);
        }

        public List<Song> GetList()
        {
            return songList;
        }

        public Song GetSong(int songID)
        {
            Song song = null;
            foreach (Song element in songList)
            {
                if (songID == element.GetSongID())
                {
                    song = element;
                    /*take a*/
                    break /*have a kitkat*/;
                }
            }

            //Returns the song.
            //If song is not found, it will return a null.
            return song;
        }

        //Used to get a relative song data.
        public Song GetRelativeSong(int songID, bool getNextSong = true)
        {
            Song song = null;
            //Determines if the song gotten will be a song after the given SongID or before.
            int x = 1;
            if (!getNextSong)
            {
                x = -1;
            }
            for (int i = 0; i < songList.Count; ++i)
            {
                if (songID == songList[i].GetSongID())
                {
                    song = songList.ElementAt(i + x);
                    break;
                }
            }

            return song;
        }
    }
}