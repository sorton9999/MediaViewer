using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLib.DataModel
{
    public class Album
    {
        public Album(string albumName)
        {
            this.AlbumName = albumName;
        }

        public string AlbumName
        {
            get;
            private set;
        }
    }
}
