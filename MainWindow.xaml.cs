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
        private List<string> file = new List<string>();
        private int i = 0;

        private void openFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Multiselect = true; //multiple fileselect
            open.Filter = "Audio File|*.mp3;";
            if (open.ShowDialog() == true)
            {
                MessageBox.Show(open.FileName);

                foreach (string files in open.FileNames)
                {
                    file.Add(files); //saves all files into list
                }
                
            }
            else
            {
                MessageBox.Show("An Error occured");
                return;
            }
        }
        private void play(object sender, RoutedEventArgs e)
        {
            if(!file.Any())
            {
                 MessageBox.Show("You need to open a file first");
                return;
            }
            if(player.PlaybackState == PlaybackState.Playing)
            {
                player.Pause(); //pause
                playBtn.Source = new BitmapImage(new Uri("assets/img/play.png", UriKind.Relative));
            }
            else if(player.PlaybackState == PlaybackState.Paused)
            {
                player.Play(); //resume
                playBtn.Source = new BitmapImage(new Uri("assets/img/pause.png", UriKind.Relative));
            }
            else { 
            WaveStream mainOutputStream = new Mp3FileReader(file[i]); //plays first item from selected music
            WaveChannel32 volumeStream = new WaveChannel32(mainOutputStream);
                volumeStream.PadWithZeroes = false; //https://stackoverflow.com/a/11280383


                player.Init(volumeStream); //Initialize WaveChannel
            
            player.PlaybackStopped += new EventHandler<StoppedEventArgs>(onPlaybackStop); //function to launch when playback stops
            player.Play(); //play

            playBtn.Source = new BitmapImage(new Uri("assets/img/pause.png", UriKind.Relative)); //change button image
            }
        }
        public void onPlaybackStop(object sender, EventArgs e)
        {
            playBtn.Source = new BitmapImage(new Uri("assets/img/play.png", UriKind.Relative));
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            stop(sender,e);
            i += 1;
            play(sender, e);
        }
        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            stop(sender, e);
            i -= 1;
            play(sender, e);
        }
        public void stop(object sender, RoutedEventArgs e)
        {
            player.Stop();

        }
    }
}
