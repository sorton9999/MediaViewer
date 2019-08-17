using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLib
{
    public class DBVersionResultSet
    {
        public int Version { get; set; }
        public int MinorVersion { get; set; }
        public int PointVersion { get; set; }
        public int Active { get; set; }
        public string Notes { get; set; }

        public string TableName;

        public DBVersionResultSet()
        {
            this.Version = 0;
            this.MinorVersion = 0;
            this.PointVersion = 0;
            this.Active = 0;
            this.Notes = String.Empty;

            this.TableName = "DBVersion";
        }

        public DBVersionResultSet(DBVersionResultSet rsIn)
        {
            this.Version = rsIn.Version;
            this.MinorVersion = rsIn.MinorVersion;
            this.PointVersion = rsIn.PointVersion;
            this.Active = rsIn.Active;
            this.Notes = rsIn.Notes;

            this.TableName = "DBVersion";
        }

        public DBVersionResultSet(int version, int minorVersion, int pointVersion, int active, string notes)
        {
            this.Version = version;
            this.MinorVersion = minorVersion;
            this.PointVersion = pointVersion;
            this.Active = active;
            this.Notes = notes;

            this.TableName = "DBVersion";
        }
    }
}
