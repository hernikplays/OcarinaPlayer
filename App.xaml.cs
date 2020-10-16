using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using CSCore.Codecs;
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
        public static String[] mArgs = {};
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Config config = Config.getConf();
            Color primary = (Color)ColorConverter.ConvertFromString(config.PrimaryColor);
            Color secondary = (Color)ColorConverter.ConvertFromString(config.SecondaryColor);
            IBaseTheme baseTheme = (config.DarkBase == false)?Theme.Light:Theme.Dark;

            ITheme theme = Theme.Create(baseTheme, primary, secondary);
            ResourceDictionaryExtensions.SetTheme(Current.Resources,theme);
            File.WriteAllText("./log.txt", "e.Args[0]");
            if (e.Args.Length > 0)
            {
                File.WriteAllText("./log.txt",e.Args[0]);
                mArgs = e.Args;
            }
        }

    }
}
