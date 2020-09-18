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
using Microsoft.Win32;
using PlaylistsNET.Models;
using PlaylistsNET.Content;
using TagLib.Mpeg;
using System.IO;
using DiscordRPC;
using DiscordRPC.Logging;
using System.ComponentModel;
using System.Windows.Threading;
using MaterialDesignThemes.Wpf;
using CSCore;
using CSCore.Codecs;
using CSCore.CoreAudioAPI;
using CSCore.SoundOut;
using System.Xml.Schema;
using Newtonsoft.Json;
using System.Net;
using System.Text.RegularExpressions;
using System.Globalization;
using SweetAlertSharp;
using SweetAlertSharp.Enums;

namespace OcarinaPlayer
{
    /// <summary>
    /// Interakční logika pro MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DiscordRpcClient client = new DiscordRpcClient("690238946378121296");

        private Config config;
        private readonly double Version = 0.4;

        public MainWindow()
        {
            InitializeComponent();
            Closed += onCloseApp;
        }
        private void OnLoad(object sender, RoutedEventArgs e)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.github.com/repos/hernikplays/OcarinaPlayer/releases/latest");
            request.UserAgent = "hernikplays";
            WebResponse response = request.GetResponse();

            using (Stream dataStream = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                string pattern = @"""tag_name"":""(?:(?=(\\?)).)*?\1""";
                Match m = Regex.Match(responseFromServer, pattern);
                var ver = float.Parse(m.Value.Replace("\"", "").Replace("tag_name:", ""), CultureInfo.InvariantCulture);
                if (ver > Version)
                {
                    var alert = new SweetAlert();
                    alert.Caption = "New version released";
                    alert.Message = "Click 'Yes' to download new version from GitHub";
                    alert.MsgImage = SweetAlertImage.INFORMATION;
                    alert.MsgButton = SweetAlertButton.YesNo;
                    alert.OkText = "Yes";
                    alert.CancelText = "No";

                    var result = alert.ShowDialog();
                    if(result.ToString() == "OK")
                    {
                        System.Diagnostics.Process.Start("https://github.com/hernikplays/OcarinaPlayer/releases/latest");
                    }
                }
            }

            response.Close();

            config = Config.getConf();

            seekbar.IsEnabled = false;

            if (config.EnableDRPC == true)
            {
                client.Logger = new ConsoleLogger() { Level = LogLevel.Warning };

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
            var folderfilelist = Directory.GetFiles(config.MusicFolderPath);
            var typeArray = CodecFactory.SupportedFilesFilterEn.Replace("Supported Files|","").Split(';');
            List<FolderListItem> musicList = new List<FolderListItem>();
            foreach (var item in folderfilelist)
            {
                foreach (var type in typeArray)
                {
                    if (item.EndsWith(type.Replace("*", "")))
                    {
                        var musobj = new FolderListItem(null,item);
                        //musobj.Path = item;
                        musicList.Add(musobj);
                    }
                }
            }
            if (!musicList.Any())
            {
                FolderList.Items.Add("None Found");
            }
            else
            {
                foreach (var song in musicList)
                {
                    var playing = TagLib.File.Create(song.Path);
                    FolderList.DisplayMemberPath = "Name";
                    if (playing.Tag.Title == null || playing.Tag.Title.Length == 0)
                    {
                        var filename = Path.GetFileName(song.Path);
                        FolderList.Items.Add(new FolderListItem(filename,song.Path));
                    }
                    else
                    {
                        var name = playing.Tag.FirstPerformer == null || playing.Tag.FirstPerformer.Length == 0 ? "Unknown" : playing.Tag.FirstPerformer + " - " + playing.Tag.Title;
                        FolderList.Items.Add(new FolderListItem(name, song.Path));
                        
                    }
                }
            }

        }

        
        public ISoundOut soundOut;
        public IWaveSource waveSource;
        private List<string> file = new List<string>();
        private List<string> unshuffledPL = new List<string>();
        DispatcherTimer aTimer;
        public int i = 0;
        public bool loop = false;
        private bool shufflePL = false;
        private bool reposition = false;
        private float pausedVol;

        private void openFile(object sender, RoutedEventArgs e)
        {
            // OPEN AND LOAD FILES
            if (shufflePL == true)
            {
                shufflePL = false;
                unshuffledPL.Clear();
                Color color = (Color)ColorConverter.ConvertFromString(config.PrimaryColor);
                shuffle.Background = new SolidColorBrush(color);

            }
            OpenFileDialog open = new OpenFileDialog()
            {
                Filter = CodecFactory.SupportedFilesFilterEn,
                Title = "Select files...",
                Multiselect = true
            };

            if (open.ShowDialog() == true)
            {
                foreach (string files in open.FileNames)
                {
                    file.Add(files); //saves all files into list
                    var playing = TagLib.File.Create(files);
                    if (playing.Tag.Title == null || playing.Tag.Title.Length == 0)
                    {
                        var filename = Path.GetFileName(files);
                        Playlist.Items.Add(filename);
                    }
                    else
                    {
                        Playlist.Items.Add(playing.Tag.FirstPerformer + " - " + playing.Tag.Title);
                    }
                }
            }
        }
        private void play(object sender, RoutedEventArgs e)
        {
            // PLAY FUNCTION
            if (!file.Any())
            {
                MessageBox.Show("You need to open a file first");
                return;
            }
            if (soundOut == null || soundOut.PlaybackState == PlaybackState.Stopped)
            {
                soundOut = new WasapiOut();
            }

            if (soundOut.PlaybackState == PlaybackState.Playing)
            {
                soundOut.Pause(); //pause
                playButton.Kind = PackIconKind.PlayArrow;
                var playing = TagLib.File.Create(file[i]);

                if (config.EnableDRPC == true)
                {
                    if (playing.Tag.Title == null || playing.Tag.Title.Length == 0)
                    {
                        var filename = Path.GetFileName(file[i]);
                        client.SetPresence(new RichPresence()
                        {

                            Details = "Listening to " + filename,
                            State = "Paused",
                            Assets = new Assets()
                            {
                                LargeImageKey = "rpcon",
                                LargeImageText = "Ocarina Music Player"
                            }

                        });
                    }
                    else
                    {
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
                    }
                }
            }
            else if (soundOut.PlaybackState == PlaybackState.Paused)
            {
                soundOut.Play(); //resume
                playButton.Kind = PackIconKind.Pause;


                var playing = TagLib.File.Create(file[i]);
                if (config.EnableDRPC == true)
                {
                    if (playing.Tag.Title == null || playing.Tag.Title.Length == 0)
                    {
                        var filename = Path.GetFileName(file[i]);
                        client.SetPresence(new RichPresence()
                        {
                            Details = "Listening to " + filename,
                            Timestamps = Timestamps.Now,
                            Assets = new Assets()
                            {
                                LargeImageKey = "rpcon",
                                LargeImageText = "Ocarina Music Player"
                            }

                        });
                    }
                    else
                    {
                        client.SetPresence(new RichPresence()
                        {
                            Details = "Listening to " + playing.Tag.Title,
                            State = "by " + playing.Tag.FirstPerformer == null || playing.Tag.FirstPerformer.Length == 0 ? playing.Tag.FirstPerformer : "Unknown",
                            Timestamps = Timestamps.Now,
                            Assets = new Assets()
                            {
                                LargeImageKey = "rpcon",
                                LargeImageText = "Ocarina Music Player"
                            }
                        });
                    }
                }
            }
            else
            {
                waveSource =
                CodecFactory.Instance.GetCodec(file[i])
                    .ToSampleSource()
                    .ToMono()
                    .ToWaveSource();



                var playing = TagLib.File.Create(file[i]);
                if (config.EnableDRPC == true)
                {
                    if (playing.Tag.Title == null || playing.Tag.Title.Length == 0)
                    {
                        var filename = Path.GetFileName(file[i]);
                        client.SetPresence(new RichPresence()
                        {
                            Details = "Listening to " + filename,
                            Timestamps = Timestamps.Now,
                            Assets = new Assets()
                            {
                                LargeImageKey = "rpcon",
                                LargeImageText = "Ocarina Music Player"
                            }
                        });

                        artistSong.Text = filename;
                    }
                    else
                    {
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
                        artistSong.Text = playing.Tag.FirstPerformer + " - " + playing.Tag.Title;
                    }
                }

                if (playing.Tag.Pictures.Length >= 1)
                {
                    MemoryStream stream = new MemoryStream(playing.Tag.Pictures[0].Data.Data);
                    BitmapFrame bmp = BitmapFrame.Create(stream);
                    albumArt.Source = bmp;
                }
                else
                {
                    albumArt.Source = new BitmapImage(new Uri("assets/img/noalbum.png", UriKind.Relative));
                }
                soundOut.Initialize(waveSource);
                soundOut.Volume = Convert.ToSingle(volumeSlider.Value);

                soundOut.Play();


                playButton.Kind = PackIconKind.Pause;
                aTimer = new DispatcherTimer();
                aTimer.Tick += (sende, e2) => updateSec(sender, e, waveSource);
                aTimer.Interval = new TimeSpan(0, 0, 1);
                aTimer.Start();

                seekbar.IsEnabled = true;
                seekbar.Value = 0;
                int mm = waveSource.GetLength().Minutes;
                int ss = waveSource.GetLength().Seconds;
                int mintosec = mm * 60;
                int seekbarSec = mintosec + ss;
                seekbar.Maximum = seekbarSec;

                soundOut.Stopped += (sende, e2) => onPlaybackStop(sender, e, waveSource, aTimer); //function to launch when playback stops
            }

        }

        private void updateSec(object sender, EventArgs e, IWaveSource mainOutputStream)
        {
            TimeSpan currentTime = mainOutputStream.GetPosition();
            var min = currentTime.Minutes;
            var sec = currentTime.Seconds;
            var seekbarSec = min * 60 + sec;

            seekbar.Value = seekbarSec;

            var thetime = mainOutputStream.GetPosition().ToString("mm\\:ss");
            var totaltime = mainOutputStream.GetLength().ToString("mm\\:ss");
            // Updating the Label which displays the current second
            cas.Content = thetime + " / " + totaltime;

            // Forcing the CommandManager to raise the RequerySuggested event
            CommandManager.InvalidateRequerySuggested();
        }

        public void onPlaybackStop(object sender, EventArgs e, IWaveSource reader, DispatcherTimer timer)
        {
            if (reposition == false)
            {
                timer.Stop();
                TimeSpan total = reader.GetLength();
                if (reader.GetPosition() == total && soundOut.PlaybackState == PlaybackState.Stopped)
                {
                    i++;
                    if (i > file.Count - 1)
                    {
                        i = 0;
                        if (loop == true)
                        {
                            RoutedEventArgs ee = new RoutedEventArgs();
                            play(sender, ee);
                        }
                    }
                    else
                    {
                        RoutedEventArgs ee = new RoutedEventArgs();
                        play(sender, ee);

                    }
                }
            }
        }
        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            i += 1;

            if (i > file.Count - 1)
            {
                i = 0;
            }
            if (soundOut != null)
            {
                soundOut.Dispose();
            }
            play(sender, e);
        }
        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            i = i - 1;
            if (i == -1)
            {
                i = file.Count - 1;
            }

            if (soundOut != null)
            {
                soundOut.Dispose();
            }

            play(sender, e);
        }
        public void stop(object sender, RoutedEventArgs e)
        {
            soundOut.Stop();
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

            if (pl.ShowDialog() != true)
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
            if (soundOut != null)
            {
                soundOut.Dispose();
            }
            if (config.EnableDRPC == true)
            {
                client.Dispose();
            }
        }

        private void volumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (soundOut == null)
            {
                pausedVol = Convert.ToSingle(volumeSlider.Value);
            }
            else
            {
                float vol = Convert.ToSingle(volumeSlider.Value);
                soundOut.Volume = vol;
            }
        }

        private void shuffle_Click(object sender, RoutedEventArgs e)
        {
            if (file.Any())
            {
                if (shufflePL == false)
                {
                    unshuffledPL.Clear();
                    i = 0;
                    shufflePL = true;
                    unshuffledPL = file;
                    file.Shuffle();
                    Color color = (Color)ColorConverter.ConvertFromString(config.SecondaryColor);
                    shuffle.Background = new SolidColorBrush(color);
                }
                else
                {
                    shufflePL = false;
                    i = unshuffledPL.IndexOf(file[i]);
                    //file.Clear(); 
                    file = unshuffledPL;
                    Color color = (Color)ColorConverter.ConvertFromString(config.PrimaryColor);
                    shuffle.Background = new SolidColorBrush(color);
                }
            }
        }

        private void Playlist_Click(object sender, MouseButtonEventArgs e)
        {
            var item = ItemsControl.ContainerFromElement(Playlist, e.OriginalSource as DependencyObject) as ListBoxItem;
            if (item != null)
            {
                i = Playlist.SelectedIndex - 1;
                RoutedEventArgs ar = new RoutedEventArgs();
                btnNext_Click(sender, ar);
            }
        }

        private void seekbar_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            reposition = true;
            soundOut.Stop();
            TimeSpan tomove = new TimeSpan(0, (int)(Math.Floor(seekbar.Value / 60)), (int)(Math.Floor(seekbar.Value % 60)));
            waveSource.SetPosition(tomove);
            soundOut.Play();
            if (aTimer.IsEnabled == false)
            {
                aTimer.Start();
            }
            aTimer.Start();
            reposition = false;
        }

        private void clearPL(object sender, RoutedEventArgs e)
        {
            if (file.Any())
            {
                file.Clear();
                Playlist.Items.Clear();
            }
            else
            {
                MessageBox.Show("List is empty");
            }
        }

        private void loop_Click(object sender, RoutedEventArgs e)
        {
            if (loop == false)
            {
                loop = true;
                Color color = (Color)ColorConverter.ConvertFromString(config.SecondaryColor);
                loopButton.Background = new SolidColorBrush(color);
            }
            else
            {
                loop = false;
                Color color = (Color)ColorConverter.ConvertFromString(config.PrimaryColor);
                loopButton.Background = new SolidColorBrush(color);
            }
        }

        private void openSettings(object sender, RoutedEventArgs e)
        {
            var settWindow = new Settings();
            settWindow.ShowDialog();
        }

        private void FolderList_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var item = ItemsControl.ContainerFromElement(FolderList, e.OriginalSource as DependencyObject) as ListBoxItem;
            if (item != null)
            {
                file.Clear();
                file.Add((FolderList.SelectedItem as FolderListItem).Path);
                Playlist.Items.Clear();
                var playing = TagLib.File.Create((FolderList.SelectedItem as FolderListItem).Path);
                if (playing.Tag.Title == null || playing.Tag.Title.Length == 0)
                {
                    var filename = Path.GetFileName((FolderList.SelectedItem as FolderListItem).Path);
                    Playlist.Items.Add(filename);
                }
                else
                {
                    Playlist.Items.Add(playing.Tag.FirstPerformer + " - " + playing.Tag.Title);
                }
                if (soundOut != null)
                    soundOut.Dispose();
                RoutedEventArgs r = new RoutedEventArgs();
                play(sender,r);
            }
        }

       
    }
}
