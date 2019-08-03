using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLib
{
    public class AbstractResultSet
    {
        public string TableName;
    }

    public class SearchPathResultSet
    {
        public string ID { get; set; }
        public string DirPath { get; set; }

        public string TableName;
        

        public SearchPathResultSet()
        {
            ID = String.Empty;
            DirPath = String.Empty;

            TableName = "SearchPaths";
        }

        public SearchPathResultSet(SearchPathResultSet rsIn)
        {
            ID = rsIn.ID;
            DirPath = rsIn.DirPath;

            TableName = "SearchPaths";
        }

        public SearchPathResultSet(string id, string dirPath)
        {
            ID = id;
            DirPath = dirPath;

            TableName = "SearchPaths";
        }
    }
}
