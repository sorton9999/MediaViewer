using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLib
{
    public class ResultSetAdapter<T> : IResultSet<T> where T : class
    {
        /// <summary>
        /// Comparison enumerations
        /// </summary>
        public enum CompareEnum { LT, LTE, EQ, GTE, GT, NE };

        /// <summary>
        /// Logical operator enumerations
        /// </summary>
        public enum LogicalOperatorEnum { AND, OR }

        /// <summary>
        /// Structure holding a column name string and whether or not
        /// it is a database key
        /// </summary>
        public struct ColumnDataStruct
        {
            public string ColumnName { get; set; }
            public bool IsKey { get; set; }
        }

        public class SelectColumnClass<S>
        {
            public ColumnDataStruct ColumnData { get; set; }
            public bool SelectColumn { get; set; }
            public S ColumnWhereValue { get; set; }
        }

        /// <summary>
        /// A list of column names
        /// </summary>
        protected List<ColumnDataStruct> columns = new List<ColumnDataStruct>();

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ResultSetAdapter(DataAccess db, string tableName)
        {
            DB = db;
            TableName = tableName;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rs">The resultset of type T</param>
        public ResultSetAdapter(T rs, DataAccess db, string tableName)
        {
            ResultSet = rs;
            DB = db;
            TableName = tableName;
            CreateColumnNameList();
        }

        /// <summary>
        /// The name of the table the given resultset is associated with
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// The resultset given as the argument to the constructor
        /// </summary>
        public T ResultSet { get; private set; }

        /// <summary>
        /// Private DataAccess Property
        /// </summary>
        private DataAccess DB
        {
            get;
            set;
        }

        /// <summary>
        /// Returns the list of column names
        /// </summary>
        /// <returns>The list containing the column names</returns>
        public List<ColumnDataStruct> GetColumnNames()
        {
            return columns;
        }

        /// <summary>
        /// We take a Property name and the object that has the value we are
        /// seeking and invoke the property to get the value.
        /// </summary>
        /// <param name="property_name">The name of the property we want to invoke</param>
        /// <param name="ob">The object whose Property we are invoking</param>
        /// <returns>The value from the invoked property</returns>
        public string InvokeResultSetProperty(string property_name, Object ob)
        {
            string ret_str = String.Empty;
            Type myType = typeof(T);

            try
            {
                ret_str = (string)myType.InvokeMember(property_name,
                                             BindingFlags.DeclaredOnly |
                                             BindingFlags.Public | BindingFlags.NonPublic |
                                             BindingFlags.Instance | BindingFlags.GetProperty, null, ob, null);
            }
            catch (Exception)
            {
            }
            return ret_str;
        }

        /// <summary>
        /// Given the resultset type T, get the defined column names that are
        /// assumed to be defined with Properties in the resultset class.
        /// </summary>
        public void CreateColumnNameList()
        {
            // Get the type and its properties
            Type myType = typeof(T);
            PropertyInfo[] myProps = myType.GetProperties();
            // Get which table columns are a primary key
            List<Boolean> keyInfo = DB.GetTableKeys(TableName);

            // Iterate through each property and get its name.  It is assumed each property
            // is a column name defined in the table which matches the TableName property.
            foreach (var item in myProps.OfType<object>().Select((x, i) => new { x, i }))
            {
                ColumnDataStruct column = new ColumnDataStruct();
                PropertyInfo prop = item.x as PropertyInfo;
                if (prop != null)
                {
                    try
                    {
                        column.ColumnName = prop.Name;
                        column.IsKey = keyInfo[item.i];
                        columns.Add(column);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

    }
}
