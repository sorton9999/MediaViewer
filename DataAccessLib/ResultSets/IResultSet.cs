using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLib
{
    public interface IResultSet<T> where T : class
    {
        /// <summary>
        /// Create a list of columns in the data table
        /// </summary>
        void CreateColumnNameList();

        /// <summary>
        /// Invoke the property given the property name
        /// </summary>
        /// <param name="property_name">The name of the property</param>
        /// <param name="ob">Any parameters associated with the property</param>
        /// <returns>A string representing the results of the invoked property</returns>
        string InvokeResultSetProperty(string property_name, Object ob);
    }
}
