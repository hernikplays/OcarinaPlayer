using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using DiscordRPC;
using DiscordRPC.Logging;
using Microsoft.Toolkit.Uwp.Notifications;

namespace OcarinaPlayer
{
    /// <summary>
    /// Interakční logika pro App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            DesktopNotificationManagerCompat.RegisterAumidAndComServer<NotifActivator>("Hernikplays.Ocarina");
            DesktopNotificationManagerCompat.RegisterActivator<NotifActivator>();
        }
    }
}
