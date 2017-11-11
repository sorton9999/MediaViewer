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

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            AddItemsToPlay((sender as MenuItem).DataContext as TreeViewItemViewModel, true);
        }

        private void AddList_Click(object sender, RoutedEventArgs e)
        {
            AddItemsToPlay((sender as MenuItem).DataContext as TreeViewItemViewModel, false);
        }

        private void AddItemsToPlay(TreeViewItemViewModel tvm, bool play)
        {
            if (tvm != null)
            {
                //List<string> playList = new List<string>();
                TitleViewModel vm = tvm as TitleViewModel;
                if (vm != null)
                {
                    // Here we have a single song title so a specific title must have been
                    // clicked in the tree.
                    // Play the song.
                    //playList.Add(vm.FilePath + "\\" + vm.FileName);
                    //MediaPlayProducerConsumer.PlayFile(playList);
                    MediaPlayProcess.AddFileToPlay(vm.FilePath + "\\" + vm.FileName, vm.SongLength, play);
                }
                else
                {
                    AlbumViewModel am = tvm as AlbumViewModel;
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
                        bool first = play;
                        foreach (var item in am.Children)
                        {
                            TitleViewModel tm = item as TitleViewModel;
                            if (tm != null)
                            {
                                //playList.Add(tm.FilePath + "\\" + tm.FileName);
                                MediaPlayProcess.AddFileToPlay(tm.FilePath + "\\" + tm.FileName, tm.SongLength, first);
                                first = false;
                            }
                        }
                        // Send the list to the player
                        //MediaPlayProducerConsumer.PlayFile(playList);
                    }
                }
            }
        }
    }
}
