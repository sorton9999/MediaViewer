using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLib.DataModel
{
    public class DBVersion
    {
        public DBVersion(uint version, uint minorVersion, uint pointVersion, int active, string notes)
        {
            this.Version = version;
            this.MinorVersion = minorVersion;
            this.PointVersion = pointVersion;
            this.Active = active;
            this.Notes = notes;
        }

        public uint Version
        {
            get;
            private set;
        }

        public uint MinorVersion
        {
            get;
            private set;
        }

        public uint PointVersion
        {
            get;
            private set;
        }

        public string Notes
        {
            get;
            private set;
        }

        public int Active
        {
            get;
        }

        public static uint GetDbVersion()
        {
            uint retVal = 0;

            DBVersionResultSet rs = new DBVersionResultSet();
            // Create the DAO using the resultset and get the config items from the DB
            DBVersionDao<DBVersionResultSet> dao = new DBVersionDao<DBVersionResultSet>(rs, rs.TableName);
            List<DBVersionResultSet> list = dao.GetAllResultsWhere(" WHERE Active=1");
            if (list.Count > 0)
            {
                uint majorVersion = (((uint)(list[0].Version)) << 8);
                uint minorVersion = (((uint)(list[0].MinorVersion)) << 4);
                uint pointVersion = (uint)(list[0].PointVersion);
                retVal = (majorVersion | minorVersion | pointVersion);
            }

            return retVal;
        }
    }
}
