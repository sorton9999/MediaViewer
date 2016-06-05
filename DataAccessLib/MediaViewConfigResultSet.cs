using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLib
{
    /// <summary>
    /// The resultset which stores rows of values stored in the
    /// MediaViewConfig table in the database.
    /// </summary>
    public class MediaViewConfigResultSet
    {
        #region Row Properties
        // Properties matching table columns in the MediaViewConfig table
        public string VlcPath { get; set; }
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
        public MediaViewConfigResultSet()
        {
            VlcPath = String.Empty;
            TableName = "MediaViewConfig";
        }

        /// <summary>
        /// Constructor -- copy
        /// </summary>
        /// <param name="rsIn">The resultset to copy</param>
        public MediaViewConfigResultSet(MediaViewConfigResultSet rsIn)
        {
            VlcPath = rsIn.VlcPath;
            TableName = "MediaViewConfig";
        }

        /// <summary>
        /// Constructor -- Creates a new resultset with the values as arguments
        /// </summary>
        /// <param name="vlcPath">Table column VlcPath value</param>
        public MediaViewConfigResultSet(string vlcPath)
        {
            VlcPath = vlcPath;
            TableName = "MediaViewConfig";
        }
        #endregion
    }
}
