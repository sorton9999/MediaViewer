using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DataAccessLib.DataModel;


namespace MediaViewer.TreeViewModel
{
    /// <summary>
    /// This class is the view model holding the collection of Albums.
    /// This is the child of the Artist collection and the parent of
    /// the Title collection.
    /// </summary>
    public class AlbumViewModel : TreeViewItemViewModel
    {
        /// <summary>
        /// Album data
        /// </summary>
        readonly Album _album;

        /// <summary>
        /// Artist data
        /// </summary>
        readonly ArtistViewModel _artist;

        /// <summary>
        /// Allows external calls to load the children of Album items
        /// </summary>
        public Action LoadAction;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="album">The album data to store</param>
        /// <param name="parentArtist">The parent Artist View Model</param>
        public AlbumViewModel(Album album, ArtistViewModel parentArtist)
            : base(parentArtist, true)
        {
            _album = album;
            _artist = parentArtist;
            // Add the LoadChildren call to the LoadAction Action
            LoadAction = new Action(LoadChildren);
        }

        /// <summary>
        /// Public Album Name property
        /// </summary>
        public string AlbumName
        {
            get { return _album.AlbumName; }
        }

        /// <summary>
        /// The child load method.  This is part of the lazy loading
        /// mechanism.
        /// </summary>
        protected override void LoadChildren()
        {
            foreach (Title title in DataStore.GetTitles(_album, _artist.ArtistName))
            {
                // For each title of the Album, load a suitable
                // image and the child Title View Model
                string imageName = FindImageName(title.FileName);
                base.Children.Add(new TitleViewModel(title, this, imageName));
            }
        }

        /// <summary>
        /// Given the title, find the appropriate image
        /// </summary>
        /// <param name="fileName">The title file name</param>
        /// <returns>The image file name</returns>
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
                    case "wma":
                        imageName = "Images\\wmaThumb.png";
                        break;
                    case "aac":
                        imageName = "Images\\aacThumb.png";
                        break;
                    case "ogg":
                        imageName = "Images\\oggThumb.png";
                        break;
                    case "m4a":
                        imageName = "Images\\m4aThumb.png";
                        break;
                    case "wav":
                        imageName = "Images\\wavThumb.png";
                        break;
                    case "ape":
                        imageName = "Images\\apeThumb.png";
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
