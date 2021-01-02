using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ocarina.Classes
{
    public class Song
    {
        public string Name { get;set; }
        public string Album { get; set; }
        public string Author { get; set; }
        public uint Position {get; set; }
        public string Path { get; set; }
    }
}
