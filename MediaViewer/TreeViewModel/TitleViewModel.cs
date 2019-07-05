using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DataAccessLib.DataModel;


namespace MediaViewer.TreeViewModel
{
    /// <summary>
    /// This class is the view model holding the collection of Titles.
    /// It is the leaf of the tree and the parent is the Album collection.
    /// </summary>
    public class TitleViewModel : TreeViewItemViewModel
    {
        /// <summary>
        /// The title data
        /// </summary>
        readonly Title _title;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="title">The title data to store</param>
        /// <param name="parentAlbum">The parent view model</param>
        /// <param name="imageName">A image name string</param>
        public TitleViewModel(Title title, AlbumViewModel parentAlbum, string imageName)
            : base(parentAlbum, true)
        {
            _title = title;
            this.ImageSourceName = imageName;
        }

        /// <summary>
        /// Public title name property
        /// </summary>
        public string TitleName
        {
            get { return _title.TitleName; }
        }

        /// <summary>
        /// Public song length property
        /// </summary>
        public string TitleLength
        {
            get { return _title.TitleLength; }
        }

        /// <summary>
        /// Public file path property
        /// </summary>
        public string FilePath
        {
            get { return _title.FilePath; }
        }

        /// <summary>
        /// Public file name property
        /// </summary>
        public string FileName
        {
            get { return _title.FileName; }
        }

        /// <summary>
        /// Public image name property
        /// </summary>
        public string ImageSourceName
        {
            get { return _title.ImageSourceName; }
            set { _title.ImageSourceName = value; }
        }
    }
}
