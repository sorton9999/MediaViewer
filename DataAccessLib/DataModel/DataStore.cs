using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SQLite;

namespace DataAccessLib.DataModel
{
    public static class DataStore
    {
        /// <summary>
        /// The data access class object that will get the Artists, Albums and Song Titles
        /// </summary>
        static private DataAccess dataAccess = new DataAccess();

        /// <summary>
        /// The list of Albums obtained from the GetAlbums call
        /// </summary>
        static private List<Album> albumStore;

        /// <summary>
        /// The list or Artists obtained from the GetArtists call
        /// </summary>
        static private List<Artist> artistStore;

        /// <summary>
        /// The list of Titles obtained from the GetTitles call
        /// </summary>
        static private List<Title> titleStore;

        /// <summary>
        /// The static call to get albums from the DB given the artist.
        /// </summary>
        /// <param name="artist">The Artist object that will be used to look up albums in the DB</param>
        /// <returns>An array of Albums associated with the given artist</returns>
        public static Album[] GetAlbums(Artist artist)
        {
            // Clear out what may be there already
            if (albumStore != null && albumStore.Count > 0)
            {
                albumStore.Clear();
            }
            albumStore = new List<Album>();
            // We use the DataAccess object's GetDataTable call with a well formed SQL string to
            // grab the Albums stored in the DB under the given Artist's name
            try
            {
                String escapedArtist = dataAccess.EscapeString(artist.ArtistName);
                using (DataTable albums = dataAccess.GetDataTable(@"SELECT DISTINCT Album FROM MusicMediaTable WHERE Artist ='" +
                        escapedArtist + "'"))
                {
                    // Go through each row of the Album table to create a list of Album names.  We add these to a
                    // List object.
                    foreach (DataRow album in albums.Rows)
                    {
                        albumStore.Add(new Album((string)album.ItemArray[0]));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return albumStore.ToArray();
        }

        /// <summary>
        /// The static call to get Artists from the DB
        /// </summary>
        /// <returns>An array of Artists</returns>
        public static Artist[] GetArtists()
        {
            // Clear out what's there already
            if (artistStore != null && artistStore.Count > 0)
            {
                artistStore.Clear();
            }
            artistStore = new List<Artist>();
            // We use the DataAccess object's GetDataTable call with a well formed SQL string to
            // grab the Artists stored in the DB.
            try
            {
                using (DataTable artists = dataAccess.GetDataTable(@"SELECT DISTINCT Artist FROM MusicMediaTable ASC"))
                {
                    // Go through each row of the Artist table to create a list of Artist names.  We add these to a
                    // List object.
                    foreach (DataRow artist in artists.Rows)
                    {
                        artistStore.Add(new Artist((string)artist.ItemArray[0]));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return artistStore.ToArray();
        }

        /// <summary>
        /// The static call to get Titles from the DB given the Album
        /// </summary>
        /// <param name="album">The Album object that will be used to look up song titles in the DB</param>
        /// <returns></returns>
        public static Title[] GetTitles(Album album, String artistName)
        {
            // clear out what's there already
            if (titleStore != null && titleStore.Count > 0)
            {
                titleStore.Clear();
            }
            titleStore = new List<Title>();
            // We use the DataAccess object's GetDataTable call with a well formed SQL string to
            // grab the Titles stored in the DB.
            try
            {
                String escapedAlbum = dataAccess.EscapeString(album.AlbumName);
                using (DataTable titles = dataAccess.GetDataTable(@"SELECT FilePath, FileName, Title, SongLength FROM MusicMediaTable WHERE Album ='" +
                        escapedAlbum + "' AND Artist = '" + artistName + "'"))
                {
                    // Go through each row of the Titles table to create a list of Title names.  We add these to a
                    // List object.
                    foreach (DataRow title in titles.Rows)
                    {
                        titleStore.Add(new Title((string)title.ItemArray[2], (string)title.ItemArray[0], (string)title.ItemArray[1], (string)title.ItemArray[3]));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return titleStore.ToArray();
        }
    }
}
