using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using CSCore.Codecs;
using Ocarina.Classes;
using Path = System.IO.Path;


namespace Ocarina
{
    /// <summary>
    /// Interakční logika pro MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<Song> SongsInMusicFolder = new List<Song>();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            /*var user = System.DirectoryServices.AccountManagement.UserPrincipal.Current.DisplayName;
            HiBlock.Text = "Hello, " + user;*/

            //* Find music folder and put songs into a list
            var folderList = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic));
            if (folderList.Length > 0)
            {
                foreach (var s in folderList)
                {
                    var typeArray = CodecFactory.SupportedFilesFilterEn.Replace("Supported Files|", "").Split(';');
                    var ret = true;
                    foreach (var type in typeArray)
                    {
                        if (s.EndsWith(type.Replace("*", "")))
                        {
                            ret = false;
                        }
                    }
                    if (ret == false)
                    {
                        var tag = TagLib.File.Create(s);
                        var song = new Song();
                        if (string.IsNullOrEmpty(tag.Tag.Title)) song.Name = Path.GetFileName(s);
                        else song.Name = tag.Tag.Title;
                        if (string.IsNullOrEmpty(tag.Tag.Album)) song.Album = "Unknown";
                        else song.Album = tag.Tag.Album;
                        if (string.IsNullOrEmpty(tag.Tag.FirstPerformer)) song.Author = "Unknown";
                        else song.Author = tag.Tag.FirstPerformer;
                        song.Position = tag.Tag.Track;
                        song.Path = s;
                        SongsInMusicFolder.Add(song);
                    }
                }
            }
        }

    }
}
