using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OcarinaPlayer
{
    class Config
    {
        private static readonly double currVer = 1.0;
        public bool EnableDRPC { get; set; }
        public string Lang { get; set; }
        public string PrimaryColor { get; set; }
        public string SecondaryColor { get; set; }
        public bool DarkBase { get; set; }
        public string MusicFolderPath { get; set; }
        public double Version { get; set; }

        public static Config getConf()
        {
            string configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OcarinaPlayer\\appconfig.json");
        //CHECK FOR CONFIG
        Config config = new Config();
            if (File.Exists(configPath) == false)
            {
                var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OcarinaPlayer");
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                config = new Config();
                config.EnableDRPC = true;
                config.Lang = "English";
                config.PrimaryColor = "#1eb6ff";
                config.SecondaryColor = "#3594ff";
                config.DarkBase = false;
                config.Version = 1.0;
                config.MusicFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                string json = JsonConvert.SerializeObject(config);
                File.WriteAllText(configPath, json);
                return config;
            }
            else
            {
                string json = File.ReadAllText(configPath);
                config = JsonConvert.DeserializeObject<Config>(json);
                if(config.Version != currVer)
                {
                    File.Delete(configPath);
                    MessageBox.Show("Regenerating config");
                    getConf();
                }
                return config;
            }
        }

        
    }
}
