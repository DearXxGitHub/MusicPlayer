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

namespace MusicPlayer
{
    //Creates song object.
    public class Song
    {
        private int songID;
        private string songTitle, songArtist;

        //Album ID will be used to grab the songImage.
        private long albumID;
        private Android.Net.Uri fileDirectory;

        public Song(int _songID, string _songTitle, string _songArtist, long _albumID, Android.Net.Uri _fileDirectory) {
            songID = _songID;
            songTitle = _songTitle;
            songArtist = _songArtist;
            albumID = _albumID;
            fileDirectory = _fileDirectory;
        }

        public Android.Net.Uri GetFileDirectory(){
            return fileDirectory;
        }

        public string GetSongTitle() {
            return songTitle;
        }

        public string GetSongArtist() {
            if (String.IsNullOrEmpty(songArtist)) {
                return "Unknown";
            }
            return songArtist;
        }

        public long GetAlbumID() {
            return albumID;
        }

        public int GetSongID() {
            return songID;
        }
    }
}