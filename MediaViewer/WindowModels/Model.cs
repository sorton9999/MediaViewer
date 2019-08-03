using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;


namespace MediaViewer
{
    public class Model
    {
        private string fullFilePath;

        public string FullFilePath
        {
            get { return fullFilePath; }
            set
            {
                fullFilePath = value;
                File = Path.GetFileName(fullFilePath);
            }
        }
        public string File { get; private set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string Year { get; set; }
        public string Comment { get; set; }
        public string Genre { get; set; }
        public string Length { get; set; }
        public BitmapImage AlbumArt { get; set; }
    }
}
