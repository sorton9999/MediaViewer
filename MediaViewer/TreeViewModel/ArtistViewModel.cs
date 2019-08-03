using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DataAccessLib.DataModel;


namespace MediaViewer.TreeViewModel
{
    /// <summary>
    /// This class is the view model holding the Artist.  This is the
    /// root of the collection and the parent of the Album collection.
    /// </summary>
    public class ArtistViewModel : TreeViewItemViewModel
    {
        /// <summary>
        /// The Artist data
        /// </summary>
        readonly Artist _artist;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="artist">The Artist data to store</param>
        public ArtistViewModel(Artist artist)
            : base(null, true)
        {
            _artist = artist;
        }

        /// <summary>
        /// Public artist name property
        /// </summary>
        public string ArtistName
        {
            get { return _artist.ArtistName; }
        }

        /// <summary>
        /// The child load method.  This is part of the
        /// lazy loading mechanism.
        /// </summary>
        protected override void LoadChildren()
        {
            foreach (var album in DataStore.GetAlbums(_artist).OrderBy(a => a.AlbumName))
            {
                // The parent is the root of the tree which holds
                // the artists.
                base.Children.Add(new AlbumViewModel(album, this));
            }
        }
    }
}
