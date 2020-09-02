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
using System.Windows.Shapes;

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
            rpc.IsChecked = (config.EnableDRPC == true) ? true : false;
            DarkCheck.IsChecked = (config.DarkBase == true) ? true : false;
            lang.SelectedItem = config.Lang;
            primary.Color = (Color)ColorConverter.ConvertFromString(config.PrimaryColor);
            secondary.Color = (Color)ColorConverter.ConvertFromString(config.SecondaryColor);
        }
    }
}
