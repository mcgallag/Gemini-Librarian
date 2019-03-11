using GameMusicEmuSharp;
using Gemini_Librarian.Data;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemini_Librarian
{
    /// <summary>
    /// Handles the playback of MusicTrack objects
    /// </summary>
    class AudioInterface
    {
        /// <summary>
        /// The MusicTrack currently being played
        /// </summary>
        public MusicTrack CurrentTrack { get; private set; }

        /// <summary>
        /// List of tracks queued for playback
        /// </summary>
        public PlayList PlayList { get; private set; }

        /// <summary>
        /// For reading the music files
        /// </summary>
        private GmeReader reader;
        /// <summary>
        /// For turning the files into sound output
        /// </summary>
        private WaveOutEvent player;

        /// <summary>
        /// Current Playback status
        /// </summary>
        public PlaybackState PlaybackState;

        /// <summary>
        /// Instantiates a new AudioInterface
        /// </summary>
        public AudioInterface()
        {
            PlaybackState = PlaybackState.Stopped;
            PlayList = new PlayList();
        }

        /// <summary>
        /// Adds a MusicTrack to the queue for playback
        /// </summary>
        /// <param name="track"></param>
        public void Enqueue(MusicTrack track)
        {
            PlayList.AddTrack(track);
        }

        /// <summary>
        /// Gets the next MusicTrack from the playlist and decompresses if necessary
        /// </summary>
        private void PrepareNextTrack()
        {
            CurrentTrack = PlayList.NextTrack();
            if (CurrentTrack.FileFormat == FileFormat.VGZ)
                CurrentTrack = MusicFileManager.ExtractVGZToTemp(CurrentTrack);
        }

        /// <summary>
        /// Resets playback position to beginning
        /// </summary>
        public void Rewind()
        {
            reader.Position = 0;
        }

        /// <summary>
        /// Begins playback of the next file in the PlayList queue
        /// </summary>
        public void Play()
        {
            // failsafe just in case we are in a paused state
            if (PlaybackState != PlaybackState.Paused)
            {
                PrepareNextTrack();

                // instantiate a wave audio player
                if (player == null)
                {
                    player = new WaveOutEvent();
                    player.PlaybackStopped += Player_PlaybackStopped;
                }
                // instantiate a new reader
                if (reader == null)
                {
                    reader = new GmeReader(CurrentTrack.FilePath);
                }

                player.Init(reader);
            }
            player.Play();
            PlaybackState = PlaybackState.Playing;
        }

        /// <summary>
        /// Pauses playback of a file.
        /// </summary>
        public void Pause()
        {
            if (PlaybackState == PlaybackState.Playing)
            {
                player.Pause();
                PlaybackState = PlaybackState.Paused;
            } else if (PlaybackState == PlaybackState.Paused)
            {
                player.Play();
                PlaybackState = PlaybackState.Playing;
            }
        }

        /// <summary>
        /// Halts playback completely and triggers PlaybackStopped event
        /// </summary>
        public void Stop()
        {
            player?.Stop();
        }

        /// <summary>
        /// Dispose of resources from playback.
        /// </summary>
        private void Player_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            PlaybackState = PlaybackState.Stopped;
            player.Dispose();
            player = null;
            reader.Dispose();
            reader = null;
            if (CurrentTrack.IsTempFile)
                System.IO.File.Delete(CurrentTrack.FilePath);
        }
    }
}
