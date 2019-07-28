using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SQLite;
using System.Windows.Forms;


namespace DataAccessLib
{
    public class DataAccess
    {
        /// <summary>
        /// Connection string used to open the DB for dat access
        /// </summary>
        String dbConnectionStr;

        /// <summary>
        /// Constructor -- 
        /// Using a default connection string
        /// </summary>
        public DataAccess()
        {
            dbConnectionStr = "Data Source=MediaDB;Pooling=true;FailIfMissing=false";
        }

        /// <summary>
        /// Constructor --
        /// Using the database name as a string.  This isn't a path to the file, but
        /// only the name of the DB file.
        /// </summary>
        /// <param name="dbFileName">The database string</param>
        public DataAccess(String dbFileName)
        {
            dbConnectionStr = String.Format("Data Source={0};Pooling=true;FailIfMissing=false", dbFileName);
        }

        /// <summary>
        /// Constructor --
        /// With connection options used to access the backend data.
        /// </summary>
        /// <param name="options">A Key-Value string dictionary of connection options</param>
        public DataAccess(Dictionary<String, String> options)
        {
            String str = String.Empty;
            foreach (KeyValuePair<String, String> opt in options)
            {
                str += String.Format("{0}={1}; ", opt.Key, opt.Value);
            }
            str = str.Trim().Substring(0, str.Length - 1);
            dbConnectionStr = str;
        }

        /// <summary>
        /// Get a list of booleans that indicate which columns are a primary key.
        /// The index of the column corresponds to the entry in the list at its
        /// index.
        /// </summary>
        /// <param name="tableName">The table containing the key info</param>
        /// <returns>The list of Booleans indicating primary keys</returns>
        public List<Boolean> GetTableKeys(string tableName)
        {
            List<Boolean> retKeys = default(List<Boolean>);
            try
            {
                using (SQLiteConnection cnn = new SQLiteConnection(dbConnectionStr))
                {
                    cnn.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand(String.Format(@"PRAGMA table_info({0})", tableName), cnn))
                    {
                        using (SQLiteDataReader dr = cmd.ExecuteReader())
                        {
                            if (dr.HasRows)
                            {
                                int count = dr.FieldCount;
                                retKeys = new List<bool>(count);
                                while (dr.Read())
                                {
                                    retKeys.Add(Convert.ToBoolean(dr["pk"]));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
            return retKeys;
        }

        /// <summary>
        /// Perform a standard query against the Database with a well formed
        /// SQL statement.  Returns data as a DataTable with rows and columns.
        /// </summary>
        /// <param name="sql">The well formed SQL to run</param>
        /// <returns>Results in the form of a DataTable</returns>
        public DataTable ExecuteQuery(string sql)
        {
            DataTable dt = new DataTable();
            try
            {
                SQLiteConnection conn = new SQLiteConnection(dbConnectionStr);
                conn.Open();
                SQLiteCommand cmd = new SQLiteCommand(conn);
                cmd.CommandText = sql;
                SQLiteDataAdapter data = new SQLiteDataAdapter(cmd);
                data.Fill(dt);
                conn.Close();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return dt;
        }

        /// <summary>
        /// Perform non-retrieval operations on the DB such as inserts, updates
        /// and deletes.
        /// </summary>
        /// <param name="sql">The well formed SQL to run</param>
        /// <returns>The number of rows updated</returns>
        public int Execute(string sql)
        {
            SQLiteConnection conn = new SQLiteConnection(dbConnectionStr);
            conn.Open();
            SQLiteCommand cmd = new SQLiteCommand(conn);
            cmd.CommandText = sql;
            int numRows = cmd.ExecuteNonQuery();
            conn.Close();
            return numRows;
        }

        /// <summary>
        /// Retrieve items from the DB using simple queries.
        /// </summary>
        /// <param name="sql">The well formed SQL to run</param>
        /// <returns>The result as a string</returns>
        public string ExecuteSimpleQuery(string sql)
        {
            SQLiteConnection conn = new SQLiteConnection(dbConnectionStr);
            conn.Open();
            SQLiteCommand cmd = new SQLiteCommand(conn);
            cmd.CommandText = sql;
            object value = cmd.ExecuteScalar();
            conn.Close();
            if (value != null)
            {
                return value.ToString();
            }
            return String.Empty;
        }

        /// <summary>
        /// Allows the programmer to easily update rows in the DB.  A null 'where' string is
        /// allowed.
        /// </summary>
        /// <param name="tableName">The table to update.</param>
        /// <param name="data">A dictionary containing Column names and their new values.</param>
        /// <param name="where">The where clause for the update statement.</param>
        /// <returns>A boolean true or false to signify success or failure.</returns>
        public bool Update(String tableName, Dictionary<String, String> data, String where)
        {
            String vals = "";
            Boolean retVal = true;
            if (data.Count >= 1)
            {
                foreach (KeyValuePair<String, String> val in data)
                {
                    vals += String.Format(" {0} = '{1}',", val.Key.ToString(), val.Value.ToString());
                }
                vals = vals.Substring(0, vals.Length - 1);
            }
            try
            {
                if (!String.IsNullOrEmpty(where))
                {
                    this.Execute(String.Format("update {0} set {1} where {2};", tableName, vals, where));
                }
                else
                {
                    this.Execute(String.Format("update {0} set {1};", tableName, vals, where));
                }
            }
            catch
            {
                retVal = false;
            }
            return retVal;
        }

        /// <summary>
        /// Allows the programmer to easily delete rows from the DB.
        /// </summary>
        /// <param name="tableName">The table from which to delete.</param>
        /// <param name="where">The where clause for the delete.</param>
        /// <returns>A boolean true or false to signify success or failure.</returns>
        public bool Delete(String tableName, String where)
        {
            Boolean retVal = true;
            try
            {
                if (!String.IsNullOrEmpty(where))
                {
                    this.Execute(String.Format("delete from {0} where {1};", tableName, where));
                }
                else
                {
                    this.Execute(String.Format("delete from {0};", tableName, where));
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                retVal = false;
            }
            return retVal;
        }

        /// <summary>
        /// Allows the programmer to easily insert into the DB
        /// </summary>
        /// <param name="tableName">The table into which we insert the data.</param>
        /// <param name="data">A dictionary containing the column names and data for the insert.</param>
        /// <param name="errorString">A error string assigned when the return value is FALSE indicating an insert error</param>
        /// <returns>A boolean true or false to signify success or failure.</returns>
        public bool Insert(String tableName, Dictionary<String, String> data, out String errorString)
        {
            String columns = "";
            String values = "";
            Boolean retVal = true;
            errorString = String.Empty;
            foreach (KeyValuePair<String, String> val in data)
            {
                columns += String.Format(" {0},", val.Key.ToString());
                values += String.Format(" '{0}',", EscapeString(val.Value));
            }
            columns = columns.Substring(0, columns.Length - 1);
            values = values.Substring(0, values.Length - 1);
            try
            {
                this.Execute(String.Format("insert into {0}({1}) values({2});", tableName, columns, values));
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message);
                errorString = e.Message;
                retVal = false;
            }
            return retVal;
        }

        /// <summary>
        /// Escapes the quote for compatability with database inserts
        /// </summary>
        /// <param name="inputStr">The string to alter</param>
        /// <returns>The altered string if quotes appear in string</returns>
        public String EscapeString(String inputStr)
        {
            String outStr = String.Empty;
            if (!String.IsNullOrEmpty(inputStr) && inputStr.Contains('\''))
                outStr = inputStr.Replace("'", "''");
            else
                outStr = inputStr;
            return outStr;
        }

        /// <summary>
        /// Allows the user to easily clear all data from a specific table.
        /// </summary>
        /// <param name="table">The name of the table to clear</param>
        /// <returns>A boolean true or false to signify success or failure</returns>
        public bool ClearTable(String table)
        {
            try
            {

                this.Execute(String.Format("delete from {0};", table));
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
