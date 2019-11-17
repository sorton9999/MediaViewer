using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLib
{
    public class PlayListViewResultSet
    {
        public string Id { get; set; }
        public string PlayListNameId { get; set; }
        public string PlayListName { get; set; }
        public string MediaId { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string SongLength { get; set; }

        public string TableName = String.Empty;

        public PlayListViewResultSet()
        {
            TableName = "PlayListView";
        }

        public PlayListViewResultSet(PlayListViewResultSet rsIn)
        {
            this.Id = rsIn.Id;
            this.PlayListNameId = rsIn.PlayListNameId;
            this.PlayListName = rsIn.PlayListName;
            this.MediaId = rsIn.MediaId;
            this.FilePath = rsIn.FilePath;
            this.FileName = rsIn.FileName;
            this.Title = rsIn.Title;
            this.Artist = rsIn.Artist;
            this.Album = rsIn.Album;
            this.SongLength = rsIn.SongLength;

            TableName = "PlayListView";
        }

        public PlayListViewResultSet(string id, string playListNameId, string playListName, string mediaId, string filePath, string fileName, string title, string artist, string album, string songLength)
        {
            this.Id = id;
            this.PlayListNameId = playListNameId;
            this.PlayListName = playListName;
            this.MediaId = mediaId;
            this.FilePath = filePath;
            this.FileName = fileName;
            this.Title = title;
            this.Artist = artist;
            this.Album = album;
            this.SongLength = songLength;

            TableName = "PlayListView";
        }
    }
}
