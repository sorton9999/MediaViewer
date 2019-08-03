﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLib.DataModel
{
    public class Title
    {
        public Title(string titleName, string titleLength, string filePath, string fileName)
        {
            this.TitleName = titleName;
            this.TitleLength = titleLength;
            this.FilePath = filePath;
            this.FileName = fileName;
        }

        public string TitleName
        {
            get;
            private set;
        }

        public string TitleLength
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

        public string ImageSourceName
        {
            get;
            set;
        }
    }
}
