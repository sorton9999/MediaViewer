using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLib
{
    /// <summary>
    /// This class is the data access object for the MediaFile table in the database.
    /// It is subclassed from the ResultSetDao abstract base class.
    /// </summary>
    /// <typeparam name="T">The type of resultset associated with the table</typeparam>
    public class MediaFileDao<T> : ResultSetDao<T> where T : class
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rs">The resultset of type T</param>
        /// <param name="tableName">The name of the database table</param>
        public MediaFileDao(T rs, string tableName)
            : base(rs, tableName)
        {
        }

        /// <summary>
        /// A convenience function that clears the table of any data it is holding
        /// </summary>
        /// <returns>Whether or not the operation succeeds</returns>
        public bool ClearTable()
        {
            return DB.ClearTable(base.TableName);
        }

        /// <summary>
        /// Create a resultset of type T, fill it and return it to the caller.  It
        /// is assumed the given DataRow type is associated with the type T.  This
        /// method needs to be overridden from ResultSetDao.
        /// </summary>
        /// <param name="data">The row of data</param>
        /// <returns>The resultset of type T</returns>
        public override T GetResultSetFromDataRow(DataRow data)
        {
            // Create the resultset which matches the type T
            MusicMediaResultSet rs = new MusicMediaResultSet();
            try
            {
                // Fill the resultset
                rs.ID = data.ItemArray[0].ToString();
                rs.FilePath = data.ItemArray[1].ToString();
                rs.FileName = data.ItemArray[2].ToString();
                rs.Title = data.ItemArray[3].ToString();
                rs.Artist = data.ItemArray[4].ToString();
                rs.Album = data.ItemArray[5].ToString();
                rs.FilePathID = data.ItemArray[6].ToString();
                rs.SongLength = data.ItemArray[7].ToString();
            }
            catch (InvalidCastException)
            {
            }
            // Return it as T
            return rs as T;
        }

        /// <summary>
        /// Public interface to insert the contents of the resultset into
        /// the database.  This method needs to be overridden from ResultSetDao.
        /// </summary>
        /// <param name="rs">The resultset ot type T</param>
        /// <returns>Whether or not the operation succeeded</returns>
        public override bool InsertResultSet(T rs)
        {
            // Assume the type and cast.  If it doesn't work, return false
            // otherwise return the outcome of the base class' invoke and
            // insert operation.
            bool retVal = false;
            MusicMediaResultSet mrs = rs as MusicMediaResultSet;
            if (mrs != null)
            {
                retVal = InvokeAndInsert(typeof(MusicMediaResultSet), new Object[] { mrs.ID, mrs.FilePath, mrs.FileName, mrs.Title, mrs.Artist, mrs.Album, mrs.FilePathID, mrs.SongLength });
            }
            return retVal;
        }
    }
}
