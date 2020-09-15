using Newtonsoft.Json;
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
using System.IO;
using System.Windows.Forms;

namespace OcarinaPlayer
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        Config config = Config.getConf();
        public Settings()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            rpc.IsChecked = (config.EnableDRPC == true);
            DarkCheck.IsChecked = (config.DarkBase == true);
            lang.SelectedValue = config.Lang;
            primary.Color = (Color)ColorConverter.ConvertFromString(config.PrimaryColor);
            secondary.Color = (Color)ColorConverter.ConvertFromString(config.SecondaryColor);
            MusicPath.Text = config.MusicFolderPath;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Config saveme = new Config();
            saveme.DarkBase = (bool)DarkCheck.IsChecked;
            saveme.EnableDRPC = (bool)rpc.IsChecked;
            saveme.Lang = lang.Text;
            saveme.PrimaryColor = "#" + primary.Color.R.ToString("X2") + primary.Color.G.ToString("X2") + primary.Color.B.ToString("X2");
            saveme.SecondaryColor = "#" + secondary.Color.R.ToString("X2") + secondary.Color.G.ToString("X2") + secondary.Color.B.ToString("X2");
            saveme.MusicFolderPath = MusicPath.Text == "" ? Environment.GetFolderPath(Environment.SpecialFolder.MyMusic):MusicPath.Text;
            saveme.Version = config.Version;
            string json = JsonConvert.SerializeObject(saveme);
            string configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OcarinaPlayer\\appconfig.json");
            File.WriteAllText(configPath, json);
            Close();
        }

        private void FolderButton_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog
            {
                Description = "Select Music Folder...",
                ShowNewFolderButton=true
            };
            dialog.ShowDialog();
            MusicPath.Text = dialog.SelectedPath;
        }
    }
}
