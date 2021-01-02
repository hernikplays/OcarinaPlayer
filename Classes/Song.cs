using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagLib;

namespace Ocarina.Classes
{
    public class Song
    {
        public string Name { get;set; }
        public string Album { get; set; }
        public string Author { get; set; }
        public uint Position {get; set; }
        public string Path { get; set; }
        public Tag Tag { get; set; }
    }
}
