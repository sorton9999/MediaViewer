using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLib
{
    /// <summary>
    /// The resultset which stores rows of values stored in the
    /// MusicMediaTable table in the database.
    /// </summary>
    public class MusicMediaResultSet
    {
        #region Row Properties
        // Properties matching table columns in the MusicMediaTable table
        public string ID { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string FilePathID { get; set; }
        public string SongLength { get; set; }
        #endregion

        #region Table Name
        /// <summary>
        /// The table name in the database
        /// </summary>
        public string TableName;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor -- Creates an empty resultset object
        /// </summary>
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
            SongLength = String.Empty;

            TableName = "MusicMediaTable";
        }

        /// <summary>
        /// Constructor -- copy
        /// </summary>
        /// <param name="rsIn">The resultset to copy</param>
        public MusicMediaResultSet(MusicMediaResultSet rsIn)
        {
            ID = rsIn.ID;
            FilePath = rsIn.FilePath;
            FileName = rsIn.FileName;
            Title = rsIn.Title;
            Artist = rsIn.Artist;
            Album = rsIn.Album;
            FilePathID = rsIn.FilePathID;
            SongLength = rsIn.SongLength;

            TableName = "MusicMediaTable";
        }

        /// <summary>
        /// Constructor -- Creates a new resultset with the values as arguments
        /// </summary>
        /// <param name="id">Table column ID value</param>
        /// <param name="filePath">Table column FilePath value</param>
        /// <param name="fileName">Table column FileName value</param>
        /// <param name="title">Table column title value</param>
        /// <param name="artist">Table column Artist value</param>
        /// <param name="album">Table column Album value</param>
        /// <param name="filePathId">Table column FilePathID value</param>
        /// <param name="songLength">Table column for Song Length</param>
        public MusicMediaResultSet(string id, string filePath, string fileName, string title, string artist, string album, string filePathId, string songLength)
        {
            this.ID = id;
            this.FilePath = filePath;
            this.FileName = fileName;
            this.Title = title;
            this.Artist = artist;
            this.Album = album;
            this.FilePathID = filePathId;
            this.SongLength = songLength;

            TableName = "MusicMediaTable";
        }
        #endregion
    }
}
