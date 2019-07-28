using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLib
{
    /// <summary>
    /// This class is the data access object for the SearchPath table in the database.
    /// It is subclassed from the ResultSetDao abstract base class.
    /// </summary>
    /// <typeparam name="T">The type of resultset associated with the table</typeparam>
    public class SearchPathDao<T> : ResultSetDao<T> where T : class
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rs">The resultset of type T</param>
        /// <param name="tableName">The name of the database table</param>
        public SearchPathDao(T rs, string tableName)
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
        /// <param name="row">The row of data</param>
        /// <returns>The resultset of type T</returns>
        public override T GetResultSetFromDataRow(System.Data.DataRow row)
        {
            // Create the resultset which matches the type T
            SearchPathResultSet rs = new SearchPathResultSet();
            try
            {
                // Fill the resultset
                rs.ID = row.ItemArray[0].ToString();
                rs.DirPath = row.ItemArray[1].ToString();
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
            bool retVal = false;
            // Assume the type and cast.  If it doesn't work, return false
            // otherwise return the outcome of the base class' invoke and
            // insert operation.
            SearchPathResultSet srs = rs as SearchPathResultSet;
            if (srs != null)
            {
                retVal = InvokeAndInsert(typeof(SearchPathResultSet), new Object[] { srs.ID, srs.DirPath });
            }
            return retVal;
        }
    }
}
