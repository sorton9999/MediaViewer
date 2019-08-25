using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLib
{
    public class PlayListNamesToMediaItemsResultSet
    {
        public string Id { get; set; }
        public string PlayListId { get; set; }
        public string MediaId { get; set; }

        public string TableName = String.Empty;

        public PlayListNamesToMediaItemsResultSet()
        {
            TableName = "PlayListNamesToMediaItems";
        }

        public PlayListNamesToMediaItemsResultSet(PlayListNamesToMediaItemsResultSet rsIn)
        {
            this.Id = rsIn.Id;
            this.PlayListId = rsIn.PlayListId;
            this.MediaId = rsIn.MediaId;

            TableName = "PlayListNamesToMediaItems";
        }

        public PlayListNamesToMediaItemsResultSet(string id, string playListId, string mediaId)
        {
            this.Id = id;
            this.PlayListId = playListId;
            this.MediaId = mediaId;

            TableName = "PlayListNamesToMediaItems";
        }
    }
}
