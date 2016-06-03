using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DataAccessLib.DataModel;


namespace MediaViewer.TreeViewModel
{
    public class ArtistViewModel : TreeViewItemViewModel
    {
        readonly Artist _artist;

        public ArtistViewModel(Artist artist)
            : base(null, true)
        {
            _artist = artist;
        }

        public string ArtistName
        {
            get { return _artist.ArtistName; }
        }

        protected override void LoadChildren()
        {
            foreach (Album album in DataAccessLib.DataModel.DataStore.GetAlbums(_artist))
            {
                base.Children.Add(new AlbumViewModel(album, this));
            }
        }
    }
}
