using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLib
{
    public class PlayListNameResultSet
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public string TableName = String.Empty;

        public PlayListNameResultSet()
        {
            this.Name = String.Empty;
            this.TableName = "PlayListNames";
        }

        public PlayListNameResultSet(PlayListNameResultSet rsIn)
        {
            this.Id = rsIn.Id;
            this.Name = rsIn.Name;

            this.TableName = "PlayListNames";
        }

        public PlayListNameResultSet(string id, string name)
        {
            this.Id = id;
            this.Name = name;

            this.TableName = "PlayListNames";
        }
    }
}
