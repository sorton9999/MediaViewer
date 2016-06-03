using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using DataAccessLib.DataModel;
using MediaViewer.TreeViewModel;

namespace MediaViewer
{
    /// <summary>
    /// Interaction logic for LibraryTreeControl.xaml
    /// </summary>
    public partial class LibraryTreeControl : UserControl
    {
        LibraryViewModel viewModel;
        public Action libraryTreeUpdateAction;

        public LibraryTreeControl()
        {
            InitializeComponent();

            libraryTreeUpdateAction = new Action(Update);

            LoadDataStore();
        }

        public void Update()
        {
            LoadDataStore();
            this.UpdateLayout();
        }

        private void LoadDataStore()
        {
            Artist[] artists = DataStore.GetArtists();
            viewModel = new LibraryViewModel(artists);
            base.DataContext = viewModel;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            List<string> playList = new List<string>();
            TitleViewModel vm = (sender as MenuItem).DataContext as TitleViewModel;
            if (vm != null)
            {
                playList.Add(vm.FilePath + "\\" + vm.FileName);
                MediaPlayProducerConsumer.PlayFile(playList);
            }
            else
            {
                AlbumViewModel am = (sender as MenuItem).DataContext as AlbumViewModel;
                if (am != null)
                {
                    foreach (var item in am.Children)
                    {
                        TitleViewModel tm = item as TitleViewModel;
                        if (tm != null)
                        {
                            playList.Add(tm.FilePath + "\\" + tm.FileName);
                        }
                    }
                    MediaPlayProducerConsumer.PlayFile(playList);
                }
            }
        }
    }
}
