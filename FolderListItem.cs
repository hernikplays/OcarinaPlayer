using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OcarinaPlayer
{
    class FolderListItem
    {
        public string Name { get; set; }
        public string Path { get; set; }

        public FolderListItem(string name, string path)
        {
            Path = path;
            Name = name;
        }
    }

}
