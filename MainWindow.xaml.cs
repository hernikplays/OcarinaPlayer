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
using PlaylistsNET.Models;
using PlaylistsNET.Content;
using TagLib.Mpeg;
using System.IO;
using NAudio.Wave;
using DiscordRPC;
using DiscordRPC.Logging;
using System.ComponentModel;
using System.Windows.Threading;

namespace OcarinaPlayer
{
    /// <summary>
    /// Interakční logika pro MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DiscordRpcClient client = new DiscordRpcClient("690238946378121296");

        public MainWindow()
        {
            InitializeComponent();
            Closed += onCloseApp;
        }
        private void OnLoad(object sender, RoutedEventArgs e)
        {
            //Set the logger
            client.Logger = new ConsoleLogger() { Level = LogLevel.Warning };

            //Subscribe to events
            client.OnReady += (senderRPC, eRPC) =>
            {
                Console.WriteLine("Received Ready from user {0}", eRPC.User.Username);
            };

            client.OnPresenceUpdate += (senderRPC, eRPC) =>
            {
                Console.WriteLine("Received Update! {0}", eRPC.Presence);
            };

            //Connect to the RPC
            client.Initialize();

            //Set the rich presence
            //Call this as many times as you want and anywhere in your code.
            client.SetPresence(new RichPresence()
            {
                Details = "Not listening to anything",
                State = "...",
                Assets = new Assets()
                {
                    LargeImageKey = "rpcon",
                    LargeImageText = "Ocarina Music Player"
                }

            });
        }

        private WaveOutEvent player = new WaveOutEvent();
        private List<string> file = new List<string>();
        public int i = 0;

        private void openFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Multiselect = true; //multiple fileselect
            open.Filter = "Audio File|*.mp3;";
            if (open.ShowDialog() == true)
            {
                MessageBox.Show(open.FileName);

                file.Clear();

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

            WaveStream mainOutputStream = new Mp3FileReader(file[i]); //plays item from selected music
            WaveChannel32 volumeStream = new WaveChannel32(mainOutputStream);

            
            if (player.PlaybackState == PlaybackState.Playing)
            {
                player.Pause(); //pause
                var playing = TagLib.File.Create(file[i]);
                client.SetPresence(new RichPresence()
                {

                    Details = "Listening to " + playing.Tag.Title,
                    State = "Paused",
                    Assets = new Assets()
                    {
                        LargeImageKey = "rpcon",
                        LargeImageText = "Ocarina Music Player"
                    }

                });
                playBtn.Source = new BitmapImage(new Uri("assets/img/play.png", UriKind.Relative));
            }
            else if(player.PlaybackState == PlaybackState.Paused)
            {
                player.Play(); //resume
                var playing = TagLib.File.Create(file[i]);
                client.SetPresence(new RichPresence()
                {
                    Details = "Listening to " + playing.Tag.Title,
                    State = "by " + playing.Tag.FirstPerformer,
                    Timestamps = Timestamps.Now,
                    Assets = new Assets()
                    {
                        LargeImageKey = "rpcon",
                        LargeImageText = "Ocarina Music Player"
                    }

                });
                
                playBtn.Source = new BitmapImage(new Uri("assets/img/pause.png", UriKind.Relative));
            }
            
            else { 
            
                volumeStream.PadWithZeroes = false; //https://stackoverflow.com/a/11280383


                player.Init(volumeStream); //Initialize WaveChannel

                
                playBtn.Source = new BitmapImage(new Uri("assets/img/pause.png", UriKind.Relative)); //change button image

                var playing = TagLib.File.Create(file[i]);
                client.SetPresence(new RichPresence()
                {
                    Details = "Listening to " +playing.Tag.Title,
                    State = "by "+playing.Tag.FirstPerformer,
                    Timestamps = Timestamps.FromTimeSpan(mainOutputStream.CurrentTime),
                    Assets = new Assets()
                    {
                        LargeImageKey = "rpcon",
                        LargeImageText = "Ocarina Music Player"

                    }

                });

                if(playing.Tag.Pictures.Length >= 1)
                {
                    MemoryStream stream = new MemoryStream(playing.Tag.Pictures[0].Data.Data);
                    BitmapFrame bmp = BitmapFrame.Create(stream);
                    albumArt.Source = bmp;
                }

                

                player.Play(); //play

                var aTimer = new DispatcherTimer();
                aTimer.Tick += (sende, e2) => updateSec(sender, e, mainOutputStream);
                aTimer.Interval = new TimeSpan(0, 0, 1);
                aTimer.Start();

                player.PlaybackStopped += (sende, e2) => onPlaybackStop(sender, e, mainOutputStream, aTimer); //function to launch when playback stops
                
            }
        }

        private void updateSec(object sender, EventArgs e, WaveStream mainOutputStream)
        {
            int hh = mainOutputStream.CurrentTime.Hours;
            int mm = mainOutputStream.CurrentTime.Minutes;
            int ss = mainOutputStream.CurrentTime.Seconds;

            var thetime = mainOutputStream.CurrentTime.ToString("mm\\:ss");
            // Updating the Label which displays the current second
            cas.Content = thetime;

            // Forcing the CommandManager to raise the RequerySuggested event
            CommandManager.InvalidateRequerySuggested();
        }

        public void onPlaybackStop(object sender, EventArgs e, WaveStream reader, DispatcherTimer timer)
        {
            timer.Stop();

            if(reader.CurrentTime == reader.TotalTime && player.PlaybackState == PlaybackState.Stopped)
            {
                i++;
                RoutedEventArgs ee = new RoutedEventArgs();
                play(sender, ee);
            }
            
            
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            i = i + 1;
            
            if (i > file.Count - 1)
            {
                i = 0;
                
            }
            player.Dispose();

            play(sender, e);
        }
        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            i = i - 1;
            if (i == -1)
            {

                i = file.Count - 1;
                
            }

            player.Dispose();
            
            play(sender, e);
        }
        public void stop(object sender, RoutedEventArgs e)
        {
            player.Stop();

        }

        private void savePL(object sender, RoutedEventArgs e)
        {
            if (!file.Any())
            {
                MessageBox.Show("You need to open some files first");
                return;
            }

            SaveFileDialog savefile = new SaveFileDialog();
            savefile.Filter = "Playlist Files|*.m3u;";
            savefile.FileName = "MyPlaylist.m3u";
            
            List<TagLib.File> filesToRead = new List<TagLib.File>();

            foreach (string files in file)
            {
                var filetoRead = TagLib.File.Create(files);
                filesToRead.Add(filetoRead);
            }

            M3uPlaylist playlist = new M3uPlaylist();
            playlist.IsExtended = true;

            int foreachindex = 0;

            foreach (var files in file)
            {
                
                playlist.PlaylistEntries.Add(new M3uPlaylistEntry()
                {
                    Album = filesToRead[foreachindex].Tag.Album,
                    AlbumArtist = filesToRead[foreachindex].Tag.FirstAlbumArtist,
                    Duration = filesToRead[foreachindex].Properties.Duration,
                    Path = files,
                    Title = filesToRead[foreachindex].Tag.Title
                });
            }

            M3uContent content = new M3uContent();
            string text = content.ToText(playlist);

            System.IO.File.WriteAllText(savefile.FileName, text);

            MessageBox.Show("Succesfully saved to" + savefile.FileName);
        }

        private void openPL(object sender, RoutedEventArgs e)
        {
            OpenFileDialog pl = new OpenFileDialog();
            pl.Filter = "Playlist Files|*.m3u;";
           
            if(pl.ShowDialog() != true)
            {
                MessageBox.Show("An Error occured");
                return;
            }

            M3uContent content = new M3uContent();
            FileStream read = new FileStream(pl.FileName, FileMode.Open);
            M3uPlaylist playlist = content.GetFromStream(read);

            file = playlist.GetTracksPaths();
        }

        public void onCloseApp(object sender, EventArgs e)
        {
            client.Dispose();
        }

        private void volumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            float vol = Convert.ToSingle(volumeSlider.Value);
            player.Volume = vol;
        }
    }
}
