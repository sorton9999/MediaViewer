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
    public class LibraryViewModel : DependencyObject
    {
        readonly ReadOnlyCollection<ArtistViewModel> _artists;

        public LibraryViewModel(Artist[] artists)
        {
            _artists = new ReadOnlyCollection<ArtistViewModel>(
                (from artist in artists
                 select new ArtistViewModel(artist))
                 .ToList());
        }

        public ReadOnlyCollection<ArtistViewModel> Artists
        {
            get { return _artists; }
        }
    }
}
