using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using DiscordRPC;
using DiscordRPC.Logging;
using MaterialDesignColors;
using MaterialDesignThemes.Wpf;

namespace OcarinaPlayer
{
    /// <summary>
    /// Interakční logika pro App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Config config = Config.getConf();
            Color primary = (Color)ColorConverter.ConvertFromString(config.PrimaryColor);
            Color secondary = (Color)ColorConverter.ConvertFromString(config.SecondaryColor);
            IBaseTheme baseTheme = (config.DarkBase == false)?Theme.Light:Theme.Dark;

            ITheme theme = Theme.Create(baseTheme, primary, secondary);
            ResourceDictionaryExtensions.SetTheme(Current.Resources,theme);

            MainWindow wnd = new MainWindow();
            if (e.Args.Length == 1)
            {
                wnd.AddCmdArgs(e.Args);
            }

            
        }
    }
}
