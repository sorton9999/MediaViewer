using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLib
{
    /// <summary>
    /// This is the implementation class which allows access to the MediaView configuration
    /// items table in the backend database.
    /// </summary>
    /// <typeparam name="T">The type of resultset associated with the table</typeparam>
    public class MediaViewConfigDao<T> : ResultSetDao<T> where T : class
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rs">The resultset of type T</param>
        /// <param name="tableName">The name of the database table</param>
        public MediaViewConfigDao(T rs, string tableName)
            : base(rs, tableName)
        {
        }

        /// <summary>
        /// A convenience function that clears the table of any data it is holding
        /// </summary>
        /// <returns>Whether or not the operation succeeds</returns>
        public bool ClearTable()
        {
            return DB.ClearTable(TableName);
        }

        /// <summary>
        /// Create a resultset of type T, fill it and return it to the caller.  It
        /// is assumed the given DataRow type is associated with the type T.  This
        /// method needs to be overridden from ResultSetDao.
        /// </summary>
        /// <param name="data">The row of data</param>
        /// <returns>The resultset of type T</returns>
        public override T GetResultSetFromDataRow(DataRow row)
        {
            MediaViewConfigResultSet rs = new MediaViewConfigResultSet();
            try
            {
                rs.VlcPath = row.ItemArray[0].ToString();
            }
            catch (InvalidCastException)
            {
            }
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
            bool retVal = false;

            MediaViewConfigResultSet mvrs = rs as MediaViewConfigResultSet;
            if (mvrs != null)
            {
                retVal = InvokeAndInsert(typeof(MediaViewConfigResultSet), new Object[] { mvrs.VlcPath });
            }
            return retVal;
        }
    }
}
