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
using Android.Media;

using MusicPlayer.AppUtil;
using System.Threading;
using Android.Graphics;
using System.Threading.Tasks;

namespace MusicPlayer
{
    [Activity(Label = "PlayMusicActivity")]
    public class PlayMusicActivity : Activity
    {
        private static bool isShuffling = false;
        private static SongData songData;
        private static Thread musicThread = new Thread(new ThreadStart(SeekBarHandler));

        //For the seekerbar
        private static SeekBar musicSeekbar;

        //0 For no repeat.
        //1 for repeat current song.
        //2 for repeating everything.
        private static byte repeatStatus = 0;

        private static MediaPlayer mediaPlayer = null;
        private static Song currentPlayingSong = null;
        private static Song displayedSong;

        private ImageView playBtn, playPrevBtn, playNextBtn, shuffleBtn, repeatBtn, songImage;

        private TextView headerTitle, songTitleDisplay;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.PlayMusic);

            ImageView backButton = FindViewById<ImageView>(Resource.Id.playMusicBackBtn);

            #region Initalize View Variables and Handlers
            backButton.Click += OnBackBtnPressed;
            playBtn = FindViewById<ImageView>(Resource.Id.playPauseBtn);
            playNextBtn = FindViewById<ImageView>(Resource.Id.playNextBtn);
            playPrevBtn = FindViewById<ImageView>(Resource.Id.playPrevBtn);
            shuffleBtn = FindViewById<ImageView>(Resource.Id.shuffleBtn);
            repeatBtn = FindViewById<ImageView>(Resource.Id.repeatBtn);
            songImage = FindViewById<ImageView>(Resource.Id.songImage);

            songTitleDisplay = FindViewById<TextView>(Resource.Id.songTitleDisplay);
            headerTitle = FindViewById<TextView>(Resource.Id.nowPlayingSongTitle);

            musicSeekbar = FindViewById<SeekBar>(Resource.Id.musicSeekerBar);

            playBtn.Click += PlayPauseMusic;
            shuffleBtn.Click += ShuffleMusic;
            playNextBtn.Click += SkipSong;
            playPrevBtn.Click += SkipSong;
            repeatBtn.Click += SetMusicRepeat;

            musicSeekbar.ProgressChanged += SeekerBarChanged;
            #endregion

            songData = new SongData();

            //If string object is sent from previous activity.
            if (Intent.GetIntExtra("SONGID", -1) != -1)
            {
                //Recieves the string object from the previous activity
                int currentSongID = Intent.GetIntExtra("SONGID", 0);

                displayedSong = songData.GetSong(currentSongID);
            }

            SetDisplay();
        }

        //Called when the seekbar is moved by user.
        private void SeekerBarChanged(object sender, SeekBar.ProgressChangedEventArgs eventArgs) {
            //Do something only if the user changes it.
            //Otherwise, does nothing.
            if (eventArgs.FromUser) {
                mediaPlayer.SeekTo(musicSeekbar.Progress);
            }
        }

        //Called when mediaplayer completes a song.
        private void MediaPlayerOnCompletion(object sender, EventArgs eventArgs) {
            //If repeat and shuffle is off.
            if (repeatStatus == 0 && !isShuffling)
            {
                //Do nothing.
                return;
            }
            //If set to repeat current song.
            else if (repeatStatus == 1)
            {
                //Plays back the song again.
                PreparePlayer(true);
            }
            //If set to repeat queue.
            else if (repeatStatus == 2)
            {
                //If set to shuffle.
                if (isShuffling)
                {
                    //Grabs a random song from the songData list and set it as the new displayed song.
                    displayedSong = songData.GetSong(songData.GetList().ElementAt(new Random().Next(0, (songData.GetList().Count - 1))).GetSongID());
                    SetDisplay();
                }
                else
                {
                    //Otherwise check if the song is at the end of queue. 
                    if (songData.GetList().ElementAt(songData.GetList().Count - 1) == displayedSong)
                    {
                        //If song is at end of queue, stop playing.
                        return;
                    }
                    else
                    {
                        //Otherwise, get the next song.
                        displayedSong = songData.GetRelativeSong(displayedSong.GetSongID());
                    }
                }
                //Plays the new song.
                PreparePlayer(true);
            }
        }

        private void PreparePlayer(bool playMusic = false) {
            try
            {
                //Clears the current seekbar thread first if its running.
                if (musicThread.IsAlive) {
                    ThreadHandler(1);
                }

                //Flushs the mediaplayer.
                if (mediaPlayer != null)
                {
                    //If the player is playing.
                    if (mediaPlayer.IsPlaying)
                    {
                        //Stops the player first.
                        mediaPlayer.Stop();
                    }
                    mediaPlayer.Reset();
                    mediaPlayer.Release();
                }
                //Set the mediaplayer source.
                mediaPlayer = new MediaPlayer();
                mediaPlayer.SetDataSource(displayedSong.GetFileDirectory().Path);
                mediaPlayer.Prepare();

                if (playMusic) {
                    mediaPlayer.Start();
                    currentPlayingSong = displayedSong;
                    Utils.ShowToast(this, "Now playing: " + displayedSong.GetSongTitle());
                    headerTitle.Text = "Now playing~ " + Utils.Truncate(currentPlayingSong.GetSongTitle(), 5) + "..";
                    mediaPlayer.Completion += MediaPlayerOnCompletion;
                    ThreadHandler(0);
                }
            } catch (Exception p) {
                //For debug purpose.
                Utils.ShowToast(this, "ERROR: " + p.Message, false, ToastLength.Long);
            }
        }


        private void SkipSong(object sender, EventArgs eventArgs)
        {
            //If the user requests to skip next song.
            // (Checks if the clicked View's ID equals to the playNextBtn's ID)
            if (((ImageView)sender).Id == playNextBtn.Id)
            {
                //If the current song is at the end of list and the user clicks on
                //the skip next button.
                if (displayedSong.GetSongID() == (songData.GetList().Count - 1))
                {
                    //Goes to the first song in the list.
                    displayedSong = songData.GetSong(0);
                }
                else
                {
                    //Gets the next song as usual.
                    displayedSong = songData.GetRelativeSong(displayedSong.GetSongID());
                }
            }
            else
            {
                //If the current song is at the start of list and the user clicks on
                //the skip previous button.
                if (displayedSong.GetSongID() == 0)
                {
                    //Goes to the last song in the list.
                    displayedSong = songData.GetSong((songData.GetList().Count - 1));
                }
                else
                {
                    //Gets the previous song as usual.
                    displayedSong = songData.GetRelativeSong(displayedSong.GetSongID(), false);
                }
            }
            SetDisplay();

            if (mediaPlayer != null)
            {
                //If the player was previously playing.
                if (mediaPlayer.IsPlaying)
                {
                    //Plays the new music.
                    PreparePlayer(true);
                    Utils.ShowToast(this, "Now playing: " + displayedSong.GetSongTitle());
                }
            }
        }

        private void ShuffleMusic(object sender, EventArgs eventArgs)
        {
            //If queue is currently shuffling
            //turn shuffle off.
            if (isShuffling)
            {
                //Changes the image of the shuffle button accordingly.

                //Resource.Drawable.shuffle_off returns an int
                //GetDrawable() converts that int to a drawable.
                shuffleBtn.SetImageDrawable(GetDrawable(Resource.Drawable.shuffle_off));
                isShuffling = false;
                Utils.ShowToast(this, "Shuffle: OFF");
            }
            else
            {
                shuffleBtn.SetImageDrawable(GetDrawable(Resource.Drawable.shuffle_on));
                isShuffling = true;
                Utils.ShowToast(this, "Shuffle: ON");
            }
        }

        private void SetMusicRepeat(object sender, EventArgs eventArgs)
        {
            //If the queue is not repeating.
            if (repeatStatus == 0)
            {
                //Make it repeat current song.

                //Changes the image of the repeat button accordingly
                repeatBtn.SetImageDrawable(GetDrawable(Resource.Drawable.repeat_one));
                repeatStatus = 1;
                Utils.ShowToast(this, "Repeat: Current song");
            }
            //If the queue is repeating current song.
            else if (repeatStatus == 1)
            {
                //Make it repeat the queue.
                repeatBtn.SetImageDrawable(GetDrawable(Resource.Drawable.repeat_all));
                repeatStatus = 2;
                Utils.ShowToast(this, "Repeat: Current queue");
            }
            else
            {
                //Otherwise, turn repeat off.
                repeatBtn.SetImageDrawable(GetDrawable(Resource.Drawable.repeat_off));
                repeatStatus = 0;
                Utils.ShowToast(this, "Repeat: OFF");
            }
        }

        private void PlayPauseMusic(object sender, EventArgs eventArgs)
        {
            //If the mediaPlayer previously played before.
            if (mediaPlayer != null)
            {
                //If a current music is playing
                if (mediaPlayer.IsPlaying)
                {
                    //If the currently playing song does not match the displayed song.
                    if (currentPlayingSong != displayedSong)
                    {
                        //plays the new song.
                        playBtn.SetImageDrawable(GetDrawable(Resource.Drawable.pause));
                        PreparePlayer(true);
                    }
                    else
                    {
                        //Otherwise, pause the music.
                        mediaPlayer.Pause();
                        playBtn.SetImageDrawable(GetDrawable(Resource.Drawable.play));
                    }
                }
                else
                {
                    //else plays the music.
                    playBtn.SetImageDrawable(GetDrawable(Resource.Drawable.pause));

                    //If the music was previously in a paused state.
                    if (mediaPlayer.CurrentPosition != 0 && currentPlayingSong == displayedSong)
                    {
                        mediaPlayer.Start();
                        headerTitle.Text = "Now playing~ " + Utils.Truncate(currentPlayingSong.GetSongTitle(), 5) + "..";
                    }
                    else
                    {
                        PreparePlayer(true);
                    }
                }
            }
            else {
                //Called mainly once (When app launches and first time playing.)
                PreparePlayer(true);
                playBtn.SetImageDrawable(GetDrawable(Resource.Drawable.pause));
            }
        }

        //Sets the display of the layout respectively base on the song.
        private void SetDisplay()
        {

            //If the mediaplayer is currently playing a song.
            if (currentPlayingSong != null && mediaPlayer != null)
            {
                if (mediaPlayer.IsPlaying)
                {
                    headerTitle.Text = "Now playing~ " + Utils.Truncate(currentPlayingSong.GetSongTitle(), 5) + "..";
                }
            }
            //If there was a song playing.
            else if (currentPlayingSong != null)
            {
                headerTitle.Text = "In queue~ " + currentPlayingSong.GetSongTitle();
            }
            else
            {
                headerTitle.Text = "Song Queue empty";
            }

            //Displays as  <songTitle> - <songArtst> format.
            //If songTitle is longer than 10 characters, cut it down.
            songTitleDisplay.Text = (Utils.Truncate(displayedSong.GetSongTitle(), 20)) + " ~ " + displayedSong.GetSongArtist();

            //Sets the shuffleButton's image coressponding to current shuffle status
            //If the queue is shuffling, set the image to shuffle is on.
            shuffleBtn.SetImageDrawable(GetDrawable(isShuffling ? Resource.Drawable.shuffle_on : Resource.Drawable.shuffle_off));

            //Gets the coverart of the song.
            Bitmap coverArt = Utils.GetAlbumArt(displayedSong.GetAlbumID(), this);
            //If the coverart exists.
            if (coverArt != null)
            {
                //Sets the songImage with the coverArt.
                songImage.SetImageBitmap(coverArt);
            }

            //If the queue is not repeating.
            if (repeatStatus == 0)
            {
                //Make the repeat-button appear as repeat is off.
                repeatBtn.SetImageDrawable(GetDrawable(Resource.Drawable.repeat_off));
            }
            //If queue is only repeating current song.
            else if (repeatStatus == 1)
            {
                repeatBtn.SetImageDrawable(GetDrawable(Resource.Drawable.repeat_one));
            }
            else
            {
                repeatBtn.SetImageDrawable(GetDrawable(Resource.Drawable.repeat_all));
            }

            //If the current playing song is displayed song
            //and the mediaplayer is playing.
            if (currentPlayingSong == displayedSong && mediaPlayer.IsPlaying)
            {
                playBtn.SetImageDrawable(GetDrawable(Resource.Drawable.pause));
                //TODO: Seekbar
            }
        }

        private void OnBackBtnPressed(object sender, EventArgs eventArgs)
        {
            Intent intent = new Intent(this, typeof(MainActivity));
            StartActivity(intent);
        }


        private void ThreadHandler(byte command) {
            if (command == 0)
            {
                //Starts the thread.
                musicThread = new Thread(new ThreadStart(SeekBarHandler));
                musicThread.Start();
            }
            else if (command == 1)
            {
                //Stops the thread.
                musicThread.Abort();
                musicThread.Join();
            }
            //Used for resuming a thread.
            else if (command == 2) {
                musicThread.Start();
            }
        }

        private static void SeekBarHandler() {
            try
            {
                if (mediaPlayer == null) {
                    return;
                }

                do
                {
                    //Handles the seekerbar and the duration display as the mediaplayer plays.
                    int currentPosition = mediaPlayer.CurrentPosition;
                    int totalDuration = mediaPlayer.Duration;
                    musicSeekbar.Max = totalDuration;
                    musicSeekbar.Progress = currentPosition;
                } while (mediaPlayer.IsPlaying);
            }
            catch (Exception p) {}
        }
    }
}