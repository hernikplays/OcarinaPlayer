using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace OcarinaPlayer
{
    /// <summary>
    /// Interakční logika pro MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private WaveOutEvent player = new WaveOutEvent();
        private string file = string.Empty;

        private void btnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "Audio File|*.mp3;";
            if (open.ShowDialog() == true)
            {
                MessageBox.Show(open.FileName);

                file = open.FileName;

            }
            else
            {
                MessageBox.Show("An Error occured");
                return;
            }
        }
        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            if(file == string.Empty)
            {
                 MessageBox.Show("You need to open a file first");
                return;
            }
            if(player.PlaybackState == PlaybackState.Playing)
            {
                player.Pause();
                playBtn.Source = new BitmapImage(new Uri("assets/img/play.png", UriKind.Relative));
            }
            else if(player.PlaybackState == PlaybackState.Paused)
            {
                player.Play();
                playBtn.Source = new BitmapImage(new Uri("assets/img/pause.png", UriKind.Relative));
            }
            else { 
            WaveStream mainOutputStream = new Mp3FileReader(file);
            WaveChannel32 volumeStream = new WaveChannel32(mainOutputStream);
                volumeStream.PadWithZeroes = false;


            player.Init(volumeStream);
            
            player.PlaybackStopped += new EventHandler<StoppedEventArgs>(onPlaybackStop);
            player.Play();

            playBtn.Source = new BitmapImage(new Uri("assets/img/pause.png", UriKind.Relative));
            }
        }
        public void onPlaybackStop(object sender, EventArgs e)
        {
            playBtn.Source = new BitmapImage(new Uri("assets/img/play.png", UriKind.Relative));
        }
    }
}
