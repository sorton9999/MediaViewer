using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

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

        private async void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            List<string> playList = new List<string>();
            TitleViewModel vm = (sender as MenuItem).DataContext as TitleViewModel;
            if (vm != null)
            {
                // Here we have a single song title so a specific title must have been
                // clicked in the tree.
                // Play the song.
                playList.Add(vm.FilePath + "\\" + vm.FileName);
                await MediaPlayWorker.PlayFileAsync(playList);
            }
            else
            {
                AlbumViewModel am = (sender as MenuItem).DataContext as AlbumViewModel;
                if (am != null)
                {
                    // Play each song title under the clicked album title.

                    // The children (song titles) are not expanded, but since the tree
                    // is using lazy loading on expansion, we need to get the children
                    // without explicily clicking on the window to expand the tree.
                    // If the album is already expanded, don't do this.
                    if (!am.IsExpanded)
                    {
                        am.LoadAction.Invoke();
                    }
                    // Go through each title and load a list with its path
                    // to get to its file to play
                    foreach (var item in am.Children)
                    {
                        TitleViewModel tm = item as TitleViewModel;
                        if (tm != null)
                        {
                            playList.Add(tm.FilePath + "\\" + tm.FileName);
                        }
                    }
                    // Send the list to the player
                    await MediaPlayWorker.PlayFileAsync(playList);
                }
            }
        }
    }
}
