using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLib.DataModel
{
    public class Title
    {
        public Title(string titleName, string filePath, string fileName, string length)
        {
            this.TitleName = titleName;
            this.FilePath = filePath;
            this.FileName = fileName;
            this.Length = length;
        }

        public string TitleName
        {
            get;
            private set;
        }

        public string FilePath
        {
            get;
            private set;
        }

        public string FileName
        {
            get;
            private set;
        }

        public string Length
        {
            get;
            private set;
        }

        public string ImageSourceName
        {
            get;
            set;
        }
    }
}
