using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLib.DataModel
{
    public class Artist
    {
        public Artist(string artistName)
        {
            this.ArtistName = artistName;
        }

        public string ArtistName
        {
            get;
            private set;
        }
    }
}
