using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DataAccessLib.DataModel;
using System.Windows;


namespace MediaViewer.TreeViewModel
{
    /// <summary>
    /// This class provides storage and access for the root of the tree, the artists.
    /// The artist view models are created and put into a collection.
    /// </summary>
    public class LibraryViewModel : DependencyObject
    {
        /// <summary>
        /// The collection of artist view models
        /// </summary>
        readonly ReadOnlyCollection<ArtistViewModel> _artists;

        /// <summary>
        /// Constructor -- The artis view models are created and put into
        /// the collection in alpha order.
        /// </summary>
        /// <param name="artists"></param>
        public LibraryViewModel(Artist[] artists)
        {
            _artists = new ReadOnlyCollection<ArtistViewModel>(
                (from artist in artists
                 select new ArtistViewModel(artist))
                 .OrderBy(s => s.ArtistName)
                 .ToList());     
        }

        /// <summary>
        /// Public Artist collection property
        /// </summary>
        public ReadOnlyCollection<ArtistViewModel> Artists
        {
            get { return _artists; }
        }
    }
}
