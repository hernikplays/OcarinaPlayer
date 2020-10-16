using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OcarinaPlayer
{
    class LanguageStrings
    {
        // please tell me there is a more optimal way to do this
        public string SongQueue { get; set; }
        public string ArtistDefault { get; set; }
        public string FileText { get; set; }
        public string EditText { get; set; }
        public string OpenFile { get; set; }
        public string OpenPl { get; set; }
        public string SavePl { get; set; }
        public string ClearQueue { get; set; }
        public string Exit { get; set; }
        public string Settings { get; set; }
        public string AllowRpc { get; set; }
        public string PrimaryColor { get; set; }
        public string SecondaryColor { get; set; }
        public string DarkTheme { get; set; }
        public string Language { get; set; }
        public string MusicFolder { get; set; }
        public string Save { get; set; }
        public string PrevThumb { get; set; }
        public string PlayThumb { get; set; }
        public string NextThumb { get; set; }

        public static LanguageStrings GetStrings(string lan)
        {
            string json = File.ReadAllText(@"./assets/lang/"+lan+".json");
            LanguageStrings lang = JsonConvert.DeserializeObject<LanguageStrings>(json);
            return lang;

        }
    }
}
