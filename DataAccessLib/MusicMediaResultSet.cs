using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLib
{
    public class MusicMediaResultSet
    {
        public string ID { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string FilePathID { get; set; }

        public string TableName;

       
        public MusicMediaResultSet()
            : base()
        {
            ID = String.Empty;
            FilePath = String.Empty;
            FileName = String.Empty;
            Title = String.Empty;
            Artist = String.Empty;
            Album = String.Empty;
            FilePathID = String.Empty;

            TableName = "MusicMediaTable";
        }

        public MusicMediaResultSet(MusicMediaResultSet rsIn)
        {
            ID = rsIn.ID;
            FilePath = rsIn.FilePath;
            FileName = rsIn.FileName;
            Title = rsIn.Title;
            Artist = rsIn.Artist;
            Album = rsIn.Album;
            FilePathID = rsIn.FilePathID;

            TableName = "MusicMediaTable";
        }

        public MusicMediaResultSet(string id, string filePath, string fileName, string title, string artist, string album, string filePathId)
        {
            this.ID = id;
            this.FilePath = filePath;
            this.FileName = fileName;
            this.Title = title;
            this.Artist = artist;
            this.Album = album;
            this.FilePathID = filePathId;

            TableName = "MusicMediaTable";
        }
    }
}
