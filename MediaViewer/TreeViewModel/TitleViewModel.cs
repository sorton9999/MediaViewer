using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DataAccessLib.DataModel;


namespace MediaViewer.TreeViewModel
{
    public class TitleViewModel : TreeViewItemViewModel
    {
        readonly Title _title;

        public TitleViewModel(Title title, AlbumViewModel parentAlbum, string imageName)
            : base(parentAlbum, true)
        {
            _title = title;
            this.ImageSourceName = imageName;
        }

        public string TitleName
        {
            get { return _title.TitleName; }
        }

        public string FilePath
        {
            get { return _title.FilePath; }
        }

        public string FileName
        {
            get { return _title.FileName; }
        }

        public string ImageSourceName
        {
            get { return _title.ImageSourceName; }
            set { _title.ImageSourceName = value; }
        }
    }
}
