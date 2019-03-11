using System;
using System.Windows;
using System.Windows.Threading;
using GameMusicEmuSharp;
using Gemini_Librarian.Data;
using Microsoft.Win32;
using NAudio.Wave;

namespace Gemini_Librarian
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // HACK - just for debugging! get this out of here and use AudioInterface! :-)
        DateTime startPlayTime;
        AudioInterface audioInterface;
        GmeReader reader;
        IWavePlayer player;

        /// <summary>
        /// Handles UI updates during playback
        /// </summary>
        DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();

            // instantiate and set up UI update timer
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1000.0);
            timer.Tick += new EventHandler(songTimer_Tick);

            // instantiate AudioInterface for playback
            audioInterface = new AudioInterface();

            // add delegate to Closing event to clean up resources
            Closing += MainWindow_Closing;
        }

        /// <summary>
        /// Cleans up any remaining resources for closing event
        /// </summary>
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (audioInterface.PlaybackState != PlaybackState.Stopped)
                audioInterface.Stop();
        }

        /// <summary>
        /// Update UI elements during playback
        /// </summary>
        private void songTimer_Tick(object sender, EventArgs e)
        {
            // HACK - figure out current play time and display
            DateTime currentTime = DateTime.Now;
            TimeSpan current = currentTime - startPlayTime;
            PlayTimeTextBlock.Text = current.ToString(@"mm\:ss");
        }

        /// <summary>
        /// Prev button event handler. Restart the current track.
        /// </summary>
        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            audioInterface.Rewind();
        }

        /// <summary>
        /// Play button event handler. Start playback
        /// </summary>
        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO - grabs a static file for debugging purposes. Do some sort of file selection/playlist manipulation here
            MusicTrack track = MusicFileManager.ReadMetaData(@"H:\temp\02 - Green Hill Zone.vgz");
            audioInterface.Enqueue(track);
            audioInterface.Play();

            // TODO - change the play button image to a pause button
            PlayButton.Click -= PlayButton_Click;
            PlayButton.Click += PauseButton_Click;
        }

        /// <summary>
        /// Pause button event handler, pauses playback
        /// </summary>
        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            audioInterface.Pause();
        }

        /// <summary>
        /// Next button event handler. Play next track.
        /// </summary>
        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            // DEBUG just temporary stuff here to text gzip extraction
            MusicTrack track = MusicFileManager.ReadMetaData(@"H:\temp\02 - Green Hill Zone.vgz");
            MusicTrack vgmTrack = MusicFileManager.ExtractVGZToTemp(track);
            audioInterface.Enqueue(vgmTrack);
            audioInterface.Play();
            return;

            throw new NotImplementedException("Next Track button - skip to next track");
        }

        // TODO - remove this completely, just for debugging and testing
        private void Timer_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // DEBUG - prompt for a file and try to play it
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "VGM Files|*.vgm;*.vgz|SPC Files|*.spc";

            bool? result = fileDialog.ShowDialog();

            if (result == true)
            {
                MusicTrack track = MusicFileManager.ReadMetaData(fileDialog.FileName);
                reader = new GmeReader(track.FilePath);
                player = new WaveOut();
                player.Init(reader);
                player.Play();
                startPlayTime = DateTime.Now;
                timer.Start();
            }
        }

        /// <summary>
        /// Menu FileExit event handler. Closes main window.
        /// </summary>
        private void Menu_FileExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
