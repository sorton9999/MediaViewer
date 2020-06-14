using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

using DataAccessLib.DataModel;
using MediaViewer.Controls;
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

            this.DataContext = UserControl1.ColorLoader.ColorView;

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
            string tag = (string)(sender as MenuItem).Tag;
            MediaPlayWorker mediaWorker = MediaPlayWorker.Instance();

            // We set some indices where titles will potentially be inserted
            mediaWorker.SetPlayerIndices(tag);

            TitleViewModel vm = (sender as MenuItem).DataContext as TitleViewModel;
            if (vm != null)
            {
                PlayListViewModel pm = new PlayListViewModel();
                pm.OrderId = 1;
                pm.ArtistName = vm.Album.Artist.ArtistName;
                pm.Song = vm.TitleName;
                pm.Path = vm.FilePath;
                pm.Length = vm.TitleLength;
                pm.File = vm.FileName;
                pm.Selected = false;
                if (mediaWorker.InsertingTitle)
                {
                    mediaWorker.InsertTitleToPlayList(mediaWorker.PlayIndex, pm);
                }
                else
                {
                    mediaWorker.AddTitleToPlayList(pm);
                }
                // Here we have a single song title so a specific title must have been
                // clicked in the tree.
                // Play the song.
                playList.Add(vm.FilePath + "\\" + vm.FileName);
                await MediaPlayWorker.PlayFileAsync(playList, mediaWorker.PlayIndex, mediaWorker.TitlePlayingNext);
                // Kludge -- This is to just set the play/pause button on the play control to Play
                PlayerControl.PlayControl().SetPlay();
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
                    int idx = -1;
                    if (mediaWorker.InsertingTitle)
                    {
                        idx = mediaWorker.PlayIndex;
                    }
                    else
                    {
                        idx = 0;
                    }
                    foreach (var item in am.Children)
                    {
                        PlayListViewModel pm = new PlayListViewModel();
                        TitleViewModel tm = item as TitleViewModel;
                        if (tm != null)
                        {
                            playList.Add(tm.FilePath + "\\" + tm.FileName);
                            pm.OrderId = idx;
                            pm.ArtistName = tm.Album.Artist.ArtistName;
                            pm.Song = tm.TitleName;
                            pm.Path = tm.FilePath;
                            pm.Length = tm.TitleLength;
                            pm.File = tm.FileName;
                            pm.Selected = false;
                            if (mediaWorker.InsertingTitle)
                            {
                                mediaWorker.InsertTitleToPlayList(idx, pm);
                            }
                            else
                            {
                                mediaWorker.AddTitleToPlayList(pm);
                            }
                        }
                        ++idx;
                    }
                    // Send the list to the player
                    await MediaPlayWorker.PlayFileAsync(playList, mediaWorker.PlayIndex, mediaWorker.TitlePlayingNext);
                    // Kludge -- This is to just set the play/pause button on the play control to Play
                    PlayerControl.PlayControl().SetPlay();
                }
            }
        }
    }
}
