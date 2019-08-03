using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLib
{
    /// <summary>
    /// The abstract base class which uses the DataAccess object to work with
    /// the database.  The default database is set to "MediaDB", but it can
    /// be changed to use another.  There is also the ResultSetAdapter class
    /// that is used to help with dat access using resultset fields and table
    /// columns
    /// </summary>
    /// <typeparam name="T">The type of resultset associated with the table</typeparam>
    public abstract class ResultSetDao<T> where T : class
    {
        /// <summary>
        /// The default Data Access object which provides data access
        /// operations.
        /// </summary>
        DataAccess db = new DataAccess("MediaDB");

        /// <summary>
        /// The resultset field to database table columns adapter
        /// </summary>
        ResultSetAdapter<T> rsAdapter;

        /// <summary>
        /// A delegate and an event which provides a databsae insert
        /// error interface.
        /// </summary>
        /// <param name="message">The insert error message string</param>
        ///
        public delegate void InsertErrorDelegate(string message);
        public event InsertErrorDelegate InsertErrorEvent;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rs">the resultset of type T</param>
        /// <param name="tableName">The table name associated with the resultset</param>
        public ResultSetDao(T rs, string tableName)
        {
            rsAdapter = new ResultSetAdapter<T>(rs, db, tableName);
            DB = db;
        }

        /// <summary>
        /// Public property for the database object
        /// </summary>
        public DataAccess DB { get; set; }

        /// <summary>
        /// Public property for the database table name
        /// </summary>
        public string TableName
        {
            get { return rsAdapter.TableName; }
            private set { rsAdapter.TableName = value; }
        }

        /// <summary>
        /// Public abstract method to be overridden by the child class.  This
        /// should create and return a resultset of type T filled with the
        /// data held within the given dataRow object.
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public abstract T GetResultSetFromDataRow(DataRow row);

        /// <summary>
        /// Public abstract method to be overridden by the child class.  This
        /// should insert the given resultset into the database set in the DB
        /// class var.
        /// </summary>
        /// <param name="rs">The resultset of type T</param>
        /// <returns>the success of the operation</returns>
        public abstract bool InsertResultSet(T rs);

        /// <summary>
        /// Return all rows in the table assigned to the table name.  The contents are
        /// returned as a List of resultsets of type T.
        /// </summary>
        /// <returns>The list of resultsets of type T</returns>
        public List<T> GetAllResults()
        {
            // A new empty list
            List<T> list = new List<T>();
            // A simple select all SQL string
            string sql = @"SELECT * FROM " + TableName;
            // Get the contents of the table, convert each
            // DataRow to a resultset, then add to the
            // list.
            using (DataTable dt = DB.ExecuteQuery(sql))
            {
                foreach (DataRow row in dt.Rows)
                {
                    T temp = GetResultSetFromDataRow(row);
                    list.Add(temp);
                }
            }
            return list;
        }

        /// <summary>
        /// A protected method used to insert the contents of a resultset into
        /// the database.  It is a generalized method that uses reflection to
        /// create the resultset of the given Type with values to be inserted
        /// given in the Object array.
        /// </summary>
        /// <param name="tp">The type of resultset to create</param>
        /// <param name="args">The values to insert</param>
        /// <returns>The success of the insert operation</returns>
        protected bool InvokeAndInsert(Type tp, Object[] args)
        {
            bool retVal = false;
            String error = String.Empty;
            List<ResultSetAdapter<T>.ColumnDataStruct> colNames = rsAdapter.GetColumnNames();
            Object obj = tp.InvokeMember(null,
                                         System.Reflection.BindingFlags.Public |
                                         System.Reflection.BindingFlags.NonPublic |
                                         System.Reflection.BindingFlags.Instance |
                                         System.Reflection.BindingFlags.CreateInstance,
                                         null,
                                         null,
                                         args);
            Dictionary<string, string> insertItems = new Dictionary<string, string>();
            foreach (var col in colNames)
            {
                if (!col.IsKey)
                    insertItems.Add(col.ColumnName, rsAdapter.InvokeResultSetProperty(col.ColumnName, obj));
            }
            if (DB != null)
            {
                retVal = DB.Insert(TableName, insertItems, out error);
            }
            if (retVal == false)
            {
                String errStr = insertItems.ToList()[0] + ": " + error;
                OnInsertError(new InsertEventArgs() { ErrorMessage = errStr });
            }
            return retVal;
        }

        /// <summary>
        /// A protected event handler that is called on an insert error.
        /// </summary>
        /// <param name="e">The insert error message</param>
        protected virtual void OnInsertError(InsertEventArgs e)
        {
            InsertErrorDelegate localEvent = InsertErrorEvent;
            if (localEvent != null)
                localEvent(e.ErrorMessage);
        }
    }

    /// <summary>
    /// A public error event args class that holds the insert error message
    /// </summary>
    public class InsertEventArgs : EventArgs
    {
        /// <summary>
        /// The insert error message
        /// </summary>
        public string ErrorMessage
        {
            get; set;
        }   
    }
}
