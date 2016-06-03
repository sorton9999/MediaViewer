using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DataAccessLib.DataModel;


namespace MediaViewer.TreeViewModel
{
    public class AlbumViewModel : TreeViewItemViewModel
    {
        readonly Album _album;

        public AlbumViewModel(Album album, ArtistViewModel parentArtist)
            : base(parentArtist, true)
        {
            _album = album;
        }

        public string AlbumName
        {
            get { return _album.AlbumName; }
        }

        protected override void LoadChildren()
        {
            foreach (Title title in DataStore.GetTitles(_album))
            {
                string imageName = FindImageName(title.FileName);
                base.Children.Add(new TitleViewModel(title, this, imageName));
            }
        }

        private string FindImageName(string fileName)
        {
            string imageName = String.Empty;
            string[] strArray = fileName.Split('.');
            if ((strArray != null) && (strArray.Count() > 1))
            {
                switch (strArray[(strArray.Length-1)])
                {
                    case "mp3":
                        imageName = "Images\\mp3Thumb.png";
                        break;
                    case "flac":
                        imageName = "Images\\flacThumb.png";
                        break;
                    default:
                        imageName = "Images\\City.png";
                        break;
                }
            }
            return imageName;
        }
    }
}
