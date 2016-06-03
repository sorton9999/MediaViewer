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
        /// The connection string used to access the backend data
        /// </summary>
        String dbConnection;

        /// <summary>
        /// Default Constructor using a default connection string
        /// </summary>
        public DataAccess()
        {
            dbConnection = "Data Source=MediaDB;Pooling=true;FailIfMissing=false";
        }

        /// <summary>
        /// A Constructor using the database name as the input string
        /// </summary>
        /// <param name="inputFile">The database string</param>
        public DataAccess(String inputFile)
        {
            dbConnection = String.Format("Data Source={0};Pooling=true;FailIfMissing=false", inputFile);
        }

        /// <summary>
        /// A Constructor taking a key-value pairing of connection options used to access
        /// the backend data.
        /// </summary>
        /// <param name="connectionOpts">A Key-Value string dictionary of connection options</param>
        public DataAccess(Dictionary<String, String> connectionOpts)
        {
            String str = String.Empty;
            foreach (KeyValuePair<String, String> opt in connectionOpts)
            {
                str += String.Format("{0}={1}; ", opt.Key, opt.Value);
            }
            str = str.Trim().Substring(0, str.Length - 1);
            dbConnection = str;
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
                using (SQLiteConnection cnn = new SQLiteConnection(dbConnection))
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
        /// Allows the programmer to run a query against the Database.
        /// </summary>
        /// <param name="sql">The SQL to run</param>
        /// <returns>A DataTable containing the result set.</returns>
        public DataTable GetDataTable(string sql)
        {
            DataTable dt = new DataTable();
            try
            {
                SQLiteConnection cnn = new SQLiteConnection(dbConnection);
                cnn.Open();
                SQLiteCommand mycommand = new SQLiteCommand(cnn);
                mycommand.CommandText = sql;
                SQLiteDataAdapter dataAdapter = new SQLiteDataAdapter(mycommand);
                dataAdapter.Fill(dt);
                cnn.Close();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return dt;
        }

        /// <summary>
        /// Allows the programmer to interact with the database for purposes other than a query.
        /// </summary>
        /// <param name="sql">The SQL to be run.</param>
        /// <returns>An Integer containing the number of rows updated.</returns>
        public int ExecuteNonQuery(string sql)
        {
            SQLiteConnection cnn = new SQLiteConnection(dbConnection);
            cnn.Open();
            SQLiteCommand mycommand = new SQLiteCommand(cnn);
            mycommand.CommandText = sql;
            int rowsUpdated = mycommand.ExecuteNonQuery();
            cnn.Close();
            return rowsUpdated;
        }

        /// <summary>
        /// Allows the programmer to retrieve single items from the DB.
        /// </summary>
        /// <param name="sql">The query to run.</param>
        /// <returns>A string.</returns>
        public string ExecuteScalar(string sql)
        {
            SQLiteConnection cnn = new SQLiteConnection(dbConnection);
            cnn.Open();
            SQLiteCommand mycommand = new SQLiteCommand(cnn);
            mycommand.CommandText = sql;
            object value = mycommand.ExecuteScalar();
            cnn.Close();
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
            Boolean returnCode = true;
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
                    this.ExecuteNonQuery(String.Format("update {0} set {1} where {2};", tableName, vals, where));
                }
                else
                {
                    this.ExecuteNonQuery(String.Format("update {0} set {1};", tableName, vals, where));
                }
            }
            catch
            {
                returnCode = false;
            }
            return returnCode;
        }

        /// <summary>
        /// Allows the programmer to easily delete rows from the DB.
        /// </summary>
        /// <param name="tableName">The table from which to delete.</param>
        /// <param name="where">The where clause for the delete.</param>
        /// <returns>A boolean true or false to signify success or failure.</returns>
        public bool Delete(String tableName, String where)
        {
            Boolean returnCode = true;
            try
            {
                if (!String.IsNullOrEmpty(where))
                {
                    this.ExecuteNonQuery(String.Format("delete from {0} where {1};", tableName, where));
                }
                else
                {
                    this.ExecuteNonQuery(String.Format("delete from {0};", tableName, where));
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                returnCode = false;
            }
            return returnCode;
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
            Boolean returnCode = true;
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
                this.ExecuteNonQuery(String.Format("insert into {0}({1}) values({2});", tableName, columns, values));
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message);
                errorString = e.Message;
                returnCode = false;
            }
            return returnCode;
        }

        /// <summary>
        /// Validate and remove disallowed characters from the input string.  Certain characters are disallowed when
        /// an DB insert operation is done on the given string such as "," or single quotes.
        /// </summary>
        /// <param name="inputStr">The string to validate</param>
        /// <returns>The changed string with disallowed characters removed</returns>
        private String ValidateString(String inputStr)
        {
            String outStr = String.Empty;
            if (!String.IsNullOrEmpty(inputStr))
            {
                String[] tmp = inputStr.Split(new char[] { ',', '\'' });
                if (tmp != null)
                {
                    outStr = String.Concat(tmp);
                }
                else
                {
                    outStr = inputStr;
                }
            }
            return outStr;
        }

        private String EscapeString(String inputStr)
        {
            // ToDo: Make this much quicker.  Iterating  through all chars is too slow!!
            String outStr = String.Empty;
            if (!String.IsNullOrEmpty(inputStr))
            {
                
                foreach (char c in inputStr)
                {
                    outStr = String.Concat(outStr, c);
                    if (c == (char)39)
                    {
                        outStr = String.Concat(outStr, (char)39);
                    }
                }
       
                /*
                char[] delimiter = new char[] { '\'' };
                String[] tmp = inputStr.Split(delimiter);
                if (tmp != null)
                {
                    if (tmp.Length == 1)
                    {
                        return tmp[0];
                    }
                    foreach (var item in tmp.OfType<object>().Select((x, i) => new { x, i }))
                    {
                        if ((item.i + 1) % 2 == 1)
                        {
                            outStr = String.Concat(outStr, (item.x as string + delimiter[0]));
                        }
                        else
                        {
                            outStr = String.Concat(outStr, (delimiter[0] + (item.x as string)));
                        }
                    }
                }
                else
                {
                    outStr = inputStr;
                }
                 * */
            }
            return outStr;
        }

        /// <summary>
        /// Allows the user to easily clear all data from a specific table.
        /// </summary>
        /// <param name="table">The name of the table to clear.</param>
        /// <returns>A boolean true or false to signify success or failure.</returns>
        public bool ClearTable(String table)
        {
            try
            {

                this.ExecuteNonQuery(String.Format("delete from {0};", table));
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
