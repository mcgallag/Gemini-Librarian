using System;
using System.Windows;
using System.Windows.Controls;
using GameMusicEmuSharp;
using NAudio.Wave;

namespace Gemini_Librarian
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // debug - just for testing the GMESharp assembly
            try
            {
                // test with Super Turrican 1-1 theme because it's my jam
                GmeReader reader = new GmeReader("turr-03.spc");
                IWavePlayer player = new WaveOut();
                player.Init(reader);
                player.Play();
            } catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException("Previous Track button - play previous track/restart current");
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException("Play/Pause button - start/suspend music playback");
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException("Next Track button - skip to next track");
        }
    }
}
