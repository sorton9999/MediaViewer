using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Threading;
using System.Diagnostics;
using DataAccessLib;
using TagLib;
using System.Collections.ObjectModel;
using MediaViewer.Utilities;
using System.Windows.Input;
using System.Windows.Controls;

namespace MediaViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Media filter list.  Files of these types will appear
        /// on the tree.
        /// </summary>
        private List<string> filterList = new List<string>()
                        {
                            "*.aac",
                            "*.ape",
                            "*.flac",
                            "*.m4a",
                            "*.mp3",
                            "*.ogg",
                            "*.wav",
                            "*.wma"
                        };

        /// <summary>
        /// The view model for the main application
        /// </summary>
        private MediaViewerViewModel mediaViewerViewModel = new MediaViewerViewModel();

        /// <summary>
        /// The database result set from the MusicMediaTable table.
        /// </summary>
        static private MusicMediaResultSet rs = new DataAccessLib.MusicMediaResultSet();

        /// <summary>
        /// The data access object responsible for data operations with the MusicMediaTable database table
        /// </summary>
        private MediaFileDao<MusicMediaResultSet> data = new MediaFileDao<MusicMediaResultSet>(rs, rs.TableName);

        /// <summary>
        /// Holds a list of error strings coming from errors with database operations
        /// </summary>
        private List<string> errorList = new List<string>();

        /// <summary>
        /// The error list view window
        /// </summary>
        private ErrorListView errorView = new ErrorListView();

        /// <summary>
        /// The tree view for the media files
        /// </summary>
        private LibraryListView libView = null;

        /// <summary>
        /// The configuration view window.  Create this immediately because
        /// its viewmodel holds needed data from the DB.
        /// </summary>
        private ConfigView configView;

        /// <summary>
        /// The database operation thread
        /// </summary>
        private Thread insertThread = null;

        /// <summary>
        /// Progress Bar Max value
        /// </summary>
        private double progressBarMax = 0.0;

        /// <summary>
        /// Progress Bar next step advance value 
        /// </summary>
        private double progressBarStep = 0.0;

        /// <summary>
        /// Progress Bar current value
        /// </summary>
        private double progressBarValue = 0.0;

        /// <summary>
        /// Mutex Lock
        /// </summary>
        private Object syncLock = new object();

        /// <summary>
        /// Play media with this object
        /// </summary>
        static MediaPlayProcess mediaPlay = null;

        /// <summary>
        /// The media is currently playing
        /// </summary>
        bool isPlaying = false;

        /// <summary>
        /// The volume control visibility flag
        /// </summary>
        bool isVolumeControlVisible = false;

        /// <summary>
        /// Running list of songs in the Play List
        /// </summary>
        static ObservableCollection<PlayListViewModel> playListItems = new ObservableCollection<PlayListViewModel>();

        /// <summary>
        /// The playlist view model for the playlist button controls
        /// </summary>
        SavedPlayListViewModel savedPlayListViewModel = new SavedPlayListViewModel();

        /// <summary>
        /// Fastforward Button Single and Double Click Actions
        /// </summary>
        private readonly SingleMultiClickAction fFwdButtonMultiClick = null;

        /// <summary>
        /// Rewind Button Single and Double Click Actions
        /// </summary>
        private readonly SingleMultiClickAction rwdButtonMultiClick = null;

        /// <summary>
        /// Hold song details for various controls
        /// </summary>
        private ViewModel songDetails = new ViewModel();


        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            // Register for the database data operation error events
            data.InsertErrorEvent += Data_InsertErrorEventHandler;
            // Register for the item tree control selected item event
            TreeViewModel.TreeViewItemViewModel.OnItemSelected += TreeViewItemViewModel_OnItemSelected;
            // Set the item source for the DB operation error window to the error string list
            errorView.ErrorList.ItemsSource = errorList;
            // Config View send parent in
            configView = new ConfigView(this);
            // Set the Max value from ProgressBar Maximum value
            progressBarMax = workProgressBar.Maximum;

            if (!CheckDbVersion())
            {
                if (MessageBox.Show("DB Version is incorrect.  Cannot Continue.", "MediaViewer - DataBase Error", MessageBoxButton.OK, MessageBoxImage.Error) == MessageBoxResult.OK)
                {
                    Exit_Button_Click(this, new RoutedEventArgs());
                }
            }

            volumeImage.DataContext = mediaViewerViewModel;
            mediaPlay = new MediaPlayProcess(this);
            volumeControl.Volume = 25;
            SetVolumeControlImage();

            flyoutBtnExpand.DataContext = mediaViewerViewModel;
            flyoutButtonContract.DataContext = mediaViewerViewModel;

            playListItems.CollectionChanged += PlayListItems_CollectionChanged;
            playList.ItemsSource = playListItems;

            savedPlayListsCB.ItemsSource = savedPlayListViewModel.SavedPlayListItems;
            deletePlayListCB.ItemsSource = savedPlayListViewModel.DeletePlayListItems;

            playTimeLabel.DataContext = songDetails;
            songDetails.TotalPlayTime = "[00:00:00]";

            fFwdButtonMultiClick = new SingleMultiClickAction(new Action(FFBtn_SingleClickAction), new Action(FFBtn_DoubleClickAction), this.Dispatcher);
            rwdButtonMultiClick = new SingleMultiClickAction(new Action(RwdBtn_SingleClickAction), new Action(RwdBtn_DoubleClickAction), this.Dispatcher);

            BindingOperations.EnableCollectionSynchronization(errorList, syncLock);

        }

        #region Properties

        /// <summary>
        /// Property holding the flag to indicate when the media tree data has changed
        /// </summary>
        public bool DataChanged
        {
            get;
            set;
        }

        /// <summary>
        /// Indicates if the main window is exiting
        /// </summary>
        public bool Exiting
        {
            get;
            private set;
        }

        /// <summary>
        /// The list of songs to play
        /// </summary>
        public static ObservableCollection<PlayListViewModel> PlayListItems
        {
            get { return playListItems; }
            set { playListItems = value; }
        }

        public static MediaPlayProcess PlayProcess
        {
            get { return mediaPlay; }
            private set { mediaPlay = value; }
        }

        #endregion

        #region Tree Event Handlers

        /// <summary>
        /// Event handler called when an item has been selected on the media tree.  The handler is always
        /// called no matter which item has been selected, but only media song title items are considered
        /// for operations in this handler.
        /// </summary>
        /// <param name="item">The item selected</param>
        void TreeViewItemViewModel_OnItemSelected(TreeViewModel.TreeViewItemViewModel item)
        {
            if (item != null)
            {
                // Cast the base item coming in to a more specific title item.  If the
                // cast is successful, continue.
                TreeViewModel.TitleViewModel title = item as TreeViewModel.TitleViewModel;
                if (title != null)
                {
                    // Load the view model for the details control using the path and file name
                    mediaDetailsControl.LoadViewModel(title.FilePath + "\\" + title.FileName);
                }
            }
        }

        /// <summary>
        /// The handler for the database operation error events
        /// </summary>
        /// <param name="message">The error message</param>
        void Data_InsertErrorEventHandler(string message)
        {
            // Add the incoming error message to the error message list
            errorList.Add(message);
        }

        /// <summary>
        /// Does the work for the database inserts.  This is the most labor intensive operation and will
        /// cycle through all the file types, get all files of that type and then insert the required
        /// information into the backend database.  If any insert errors happen, they are inserted into
        /// the error viewer.
        /// </summary>
        private void InsertThreadHandler()
        {
            // Get the directories where this operation should look for media.  The directories are set by
            // the user.
            List<SearchPathResultSet> dirList = GetMediaPathList();

            // Keep a running count of files processed
            int runningFileCount = 0;

            // The total files counted in first pass
            int maxFileCount = 0;

            // We make two passes, the first is to count the total files to process
            bool firstPass = true;

            // Did we start inserting songs
            bool insertStarted = false;

            // Is the insert operation finished
            bool done = false;

            do
            {
                // Cycle through all the user set directories
                foreach (var dir in dirList)
                {
                    System.Diagnostics.Debug.WriteLine("{0} DIR \'{1}\'", (firstPass ? "COUNTING" : "PROCESSING"), dir.DirPath);
                    // Cycle through each file type defined in the filerList class var.
                    foreach (var filterType in filterList)
                    {
                        System.Diagnostics.Debug.WriteLine("{0} File Type: {1}", (firstPass ? "COUNTING" : "PROCESSING"), filterType);
                        // Obtain the directory info for the current directory
                        System.IO.DirectoryInfo fileDirInfo = new System.IO.DirectoryInfo(dir.DirPath);
                        if (fileDirInfo != null)
                        {
                            // Let's get all the files together into a list
                            List<System.IO.FileInfo> fileList = fileDirInfo.EnumerateFiles(filterType, System.IO.SearchOption.AllDirectories).ToList();

                            // Filter out the desired files in the directory and iterate through each one
                            foreach (var file in fileList)
                            {
                                if (!firstPass)
                                {
                                    insertStarted = true;
                                    // Grab the directory path and file name to grab the embedded data in the media file.
                                    // This uses the open source C# taglib library.
                                    string dirStr = file.DirectoryName;
                                    TagLib.File mediaInfo = null;
                                    try
                                    {
                                        mediaInfo = TagLib.File.Create(dirStr + "\\" + file);
                                    }
                                    catch (TagLib.UnsupportedFormatException ex)
                                    {
                                        Console.WriteLine("[[[EXCEPTION]]] -- Unsupported File: " + ex.Message);
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine("[Non-specific Exception]: " + e.Message);
                                    }
                                    // If we get the media data object, grab the info then insert it into
                                    // the database.
                                    if (mediaInfo != null)
                                    {
                                        // File location info
                                        rs.FilePath = dirStr;
                                        rs.FileName = file.Name;
                                        // Media file info.  This is a small subset of the total available
                                        rs.Artist = mediaInfo.Tag.FirstPerformer;
                                        rs.Title = mediaInfo.Tag.Title;
                                        rs.Album = mediaInfo.Tag.Album;
                                        rs.SongLength = ComputeSongLength(mediaInfo);
                                        // Database table foreign key
                                        rs.FilePathID = dir.ID;

                                        // Insert the data for the media file.  If we get a failure, there should be an error
                                        // from the failed operation.  Start the error viewer so we can attempt to find the
                                        // cause.
                                        if (!data.InsertResultSet(rs))
                                        {
                                            CheckAndInvoke(new Action(MakeErrorViewVisible));
                                        }
                                    }

                                    ++runningFileCount;

                                    // Advance progress bar a bit after processing this file
                                    progressBarStep = (runningFileCount % (maxFileCount / 100) == 0) ? 1 : 0;
                                    CheckAndInvoke(new Action(AdvanceProgressBar));
                                }
                                else
                                {
                                    ++maxFileCount;
                                    if ((maxFileCount %  10) == 0)
                                    {
                                        System.Diagnostics.Debug.WriteLine("Found {0} Files to Insert", maxFileCount);
                                    }
                                }
                            }

                        }
                    }
                }
                // First pass is done here.  Set to FALSE to start inserting songs
                if (firstPass)
                {
                    firstPass = false;
                    System.Diagnostics.Debug.WriteLine("FOUND {0} TOTAL FILES TO INSERT", maxFileCount);
                }

                done = ((firstPass == false) && (insertStarted == true));

            } while (!done);

            // Advance progress bar to max if needed
            CheckAndInvoke(new Action(ProgressBarStop));

            // A small notification the work has finished
            MessageBox.Show("Media Import Is Complete");

            // Reload the tree once all work has finished and the above dialog has been acknowledged.
            CheckAndInvoke(libraryTreeControl.libraryTreeUpdateAction);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Find the song duration given the media tag file
        /// </summary>
        /// <param name="mediaInfo">Media Tag File</param>
        /// <returns>The string representing the duration</returns>
        public static string ComputeSongLength(File mediaInfo)
        {
            TimeSpan length = mediaInfo.Properties.Duration;
            long lengthSec = (long)(length.TotalSeconds % 60);
            long lengthMin = (long)length.TotalMinutes;

            return (lengthMin.ToString() + ":" + lengthSec.ToString("00"));
        }

        /// <summary>
        /// A thread safe means to calling the given action
        /// </summary>
        /// <param name="action">The action to invoke</param>
        public void CheckAndInvoke(Action action)
        {
            if (!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke(action);
            }
            else
            {
                action.Invoke();
            }
        }

        /// <summary>
        /// Sets the image on the volume button to match the
        /// volume setting in the Volume Control.  The image
        /// is dynamically changed as the volume slider in
        /// the control is moved.
        /// </summary>
        public void SetVolumeControlImage()
        {
            mediaViewerViewModel.Volume = (int)volumeControl.Volume;
            if (volumeControl.Mute)
            {
                mediaViewerViewModel.ImageFile = "/Images/mute-white.png";
                mediaViewerViewModel.Volume = 0;
            }
            else if (volumeControl.Volume <= 25)
            {
                mediaViewerViewModel.ImageFile = "/Images/volume_low-white.png";
            }
            else if (volumeControl.Volume > 25 && volumeControl.Volume < 60)
            {
                mediaViewerViewModel.ImageFile = "/Images/volume_med-white.png";
            }
            else
            {
                mediaViewerViewModel.ImageFile = "/Images/volume_high-white.png";
            }
        }

        public static void SetTrackItems(int trackIdx)
        {
            foreach (var item in playListItems.ToList().Select((e, i) => new { e , i}))
            {
                if (item.i != trackIdx)
                {
                    item.e.NowPlaying = false;
                    item.e.Selected = false;
                }
                else
                {
                    item.e.NowPlaying = true;
                    item.e.Selected = true;
                }
            }
        }

        /// <summary>
        /// Used to find if the process named 'processName' is listed in
        /// the process table.  Its listing indicates whether or not it
        /// is running at the time of the call.
        /// </summary>
        /// <param name="processName">The name of the process to check</param>
        /// <returns></returns>
        public static bool IsProcessRunning(string processName)
        {
            System.Diagnostics.Process[] processes =
                System.Diagnostics.Process.GetProcessesByName(processName);
            return (processes.Length > 0);
        }

        /// <summary>
        /// Calculate the time given the seconds and return a string
        /// in the format "00:00:00"
        /// </summary>
        /// <param name="seconds">The seconds to calculate</param>
        /// <returns>The string in format "00:00:00"</returns>
        public static string SecondsToString(long seconds)
        {
            string retStr = "00:00:00";
            int totalSecs = (int)(seconds % 60);
            int totalMins = (int)((seconds / 60) % 60);
            int totalHours = (int)(((seconds / 60) / 60) % 60);
            retStr = String.Format("{0:00}:{1:00}:{2:00}", totalHours, totalMins, totalSecs);
            return retStr;
        }

        /// <summary>
        /// Change the input string in the either the format "00:00:00" or "00:00"
        /// to seconds.
        /// </summary>
        /// <param name="time">The input string in the format "00:00:00"</param>
        /// <returns>The number of seconds the string represents</returns>
        public static long StringToSeconds(string time)
        {
            long retVal = 0;
            bool one = true;
            try
            {
                // This call is to check if more than one ":" is in the string. If there is
                // the exception is triggered and the hours are parsed in the next try
                // block.
                char c = time.Single((s) => s == ':');
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Exception: {0}", e.Message);
                one = false;
            }
            try
            {
                int hrs = 0, mins = 0, secs = 0;
                string[] parts = time.Split(':');
                if (one)
                {
                    mins = Convert.ToInt32(parts[0]);
                    secs = Convert.ToInt32(parts[1]);
                    retVal = (mins * 60) + secs;
                }
                else
                {
                    hrs = Convert.ToInt32(parts[0]);
                    mins = Convert.ToInt32(parts[1]);
                    secs = Convert.ToInt32(parts[2]);
                    retVal = (hrs * 3600) + (mins * 60) + secs;
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Exception: {0}", e.Message);
            }
            return retVal;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Advance the media search progress bar as work is being done
        /// </summary>
        private void AdvanceProgressBar()
        {
            workProgressBar.Value += progressBarStep;
            progressBarValue = workProgressBar.Value;
        }

        /// <summary>
        /// Advance the progress bar to its maximum value to indicate completion
        /// </summary>
        private void ProgressBarStop()
        {
            workProgressBar.Value = workProgressBar.Maximum;
        }

        /// <summary>
        /// As a separate operation, the user can add directories that will be searched for media files
        /// to be added to the media tree control.  This list of resultsets is returned to the caller.
        /// </summary>
        /// <returns>The list of resultsets</returns>
        private List<SearchPathResultSet> GetMediaPathList()
        {
            SearchPathResultSet rs = new SearchPathResultSet();
            SearchPathDao<SearchPathResultSet> data = new SearchPathDao<SearchPathResultSet>(rs, rs.TableName);
            return data.GetAllResults();
        }

        /// <summary>
        /// Sets the visibility flag of the error view window to make it visible.  Also resets the error list
        /// to update the errors displayed in the window.
        /// </summary>
        private void MakeErrorViewVisible()
        {
            if (errorView.Visibility != System.Windows.Visibility.Visible)
            {
                errorView.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                errorView.ErrorList.ItemsSource = null;
                errorView.ErrorList.ItemsSource = errorList;
            }
        }

        private bool CheckDbVersion()
        {
            bool retVal = false;

            uint dbVersion = (CurrentDBVersion.CurrentMajorVersion | CurrentDBVersion.CurrentMinorVersion | CurrentDBVersion.CurrentPointVersion);
            retVal = DBVersionUtility.IsDbVersion(dbVersion);

            return retVal;
        }

        #endregion

        #region Control Event Handlers

        /// <summary>
        /// The handler for button clicks on the "Find Media" button.  The backend
        /// Music Media table is cleared as well as the error list.  The DB insert
        /// thread is then created and started.
        /// </summary>
        /// <param name="sender">The control sending this event being handled</param>
        /// <param name="e">The event arguments</param>
        private void Media_Button_Click(object sender, RoutedEventArgs e)
        {
            // If nothing has changed, don't do anything
            if (!DataChanged)
            {
                e.Handled = false;
                return;
            }
            // Make media search progress bar visible and set progress to zero
            if (workProgressBar.Visibility != System.Windows.Visibility.Visible)
            {
                workProgressBar.Visibility = System.Windows.Visibility.Visible;
            }
            workProgressBar.Value = 0.0;
            // Create the DB insert thread
            insertThread = new Thread(new ThreadStart(InsertThreadHandler));
            // Clear out any old data
            data.ClearTable();
            this.errorList.Clear();
            DataChanged = false;
            // Start the insert thread
            insertThread.Start();
            e.Handled = true;
        }

        /// <summary>
        /// The Exit button click event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Exit_Button_Click(object sender, RoutedEventArgs e)
        {
            Exiting = true;
            errorView.Close();
            configView.Close();
            if (libView != null)
                libView.Close();
            Close();
        }

        /// <summary>
        /// The Library button click event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Library_Button_Click(object sender, RoutedEventArgs e)
        {
            if ((libView != null) && libView.IsVisible)
            {
                return;
            }
            libView = new LibraryListView(this);
            libView.Show();
        }

        /// <summary>
        /// The Config button Click handler which calls up the Config window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Config_Button_Click(object sender, RoutedEventArgs e)
        {
            if (configView.IsVisible)
            {
                return;
            }
            configView.Show();
        }

        /// <summary>
        /// Window closing event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Exiting = true;
            errorView.Close();
            configView.Close();
            if (libView != null)
            {
                libView.Close();
            }
            if (mediaPlay != null)
            {
                mediaPlay.Dispose();
            }
        }

        /// <summary>
        /// When items are added or removed from the PlayListItem itemsource they get
        /// processed here.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlayListItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Added items
            if (e.NewItems != null && e.NewItems.Count > 0)
            {
                if (fromSavedPlayList)
                {
                    List<PlayListViewModel> list = new List<PlayListViewModel>();
                    foreach (PlayListViewModel item in e.NewItems)
                    {
                        list.Add(item);
                    }
                    foreach (PlayListViewModel item in list.OrderBy((p) => p.OrderId).ToList())
                    {
                        mediaPlay.InvokeAdder(item.Path + "\\" + item.File);
                        System.Threading.Thread.Sleep(200);
                    }
                }

                // Compute Time Length
                foreach (PlayListViewModel item in e.NewItems)
                {
                    totalSecs += StringToSeconds(item.Length);
                }

            }
            // Removed items
            if (e.OldItems != null && e.OldItems.Count > 0)
            {
                foreach (PlayListViewModel item in e.OldItems)
                {
                    mediaPlay.InvokeRemover(e.OldStartingIndex);

                    // Compute new song length
                    totalSecs -= StringToSeconds(item.Length);
                }
            }
            // Create formatted string of total play time
            songDetails.TotalPlayTime = "[" + SecondsToString(totalSecs) + "]";
        }

        long totalSecs = 0;

        /// <summary>
        /// PlayList listview right-click delete menuitem event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemDelete_Click(object sender, RoutedEventArgs e)
        {
            if (playList.SelectedIndex == -1)
            {
                return;
            }
            if (!PlayListItems[playList.SelectedIndex].NowPlaying)
            {
                playListItems.RemoveAt(playList.SelectedIndex);
            }
        }

        /// <summary>
        /// PlayList listview mouse down event handler.  This will unselect
        /// all the items if a click is registered outside of the list area.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListView_MouseDown(object sender, MouseButtonEventArgs e)
        {
            playList.UnselectAll();
        }

        #region Save PlayList Event Handlers

        int saveBtnClickNum = 0;
        int selectedIdx = -1;
        /// <summary>
        /// Event handler which responds to the Save PlayList button clicks.  The first time
        /// the button is clicked, a textbox is popped up to enter a playlist name to save
        /// the contents of the playlist.  The second time it is clicked will do the saving.
        /// If the button is clicked without entering a name, the textbox is popped down.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SavePlayListBtn_Click(object sender, RoutedEventArgs e)
        {
            ++saveBtnClickNum;
            if (saveBtnClickNum == 1)
            {
                playListSaveTB.Visibility = Visibility.Visible;
            }
            else if ((saveBtnClickNum == 2) && !String.IsNullOrEmpty(playListSaveTB.Text) && (playList.Items.Count > 0))
            {
                // Second click requires a entered name and items in the playlist

                // Set up the playlist name to enter
                PlayListNameResultSet rs = new PlayListNameResultSet();
                rs.Name = playListSaveTB.Text;
                PlayListNameDao<PlayListNameResultSet> dao = new PlayListNameDao<PlayListNameResultSet>(rs, rs.TableName);
                if (dao.InsertResultSet(rs))
                {
                    // Once the name is entered into the names table, we need to set up the song items to insert

                    // Capture some needed ID values
                    string songId = String.Empty;
                    string listId = String.Empty;

                    // Go through the playlist items storage
                    foreach (PlayListViewModel item in playList.ItemsSource)
                    {
                        // We need to get the song IDs from the DB
                        string title = DataAccess.EscapeString(item.Song);
                        MusicMediaResultSet mrs = new MusicMediaResultSet();
                        MediaFileDao<MusicMediaResultSet> mdao = new MediaFileDao<MusicMediaResultSet>(mrs, mrs.TableName);
                        List<MusicMediaResultSet> results = mdao.GetAllResultsWhere(@" WHERE Title='" + title + "'");
                        if (results.Count > 0)
                        {
                            songId = results[0].ID;

                            // Once we get the song ID, we need the playlist name ID from the DB table
                            if (String.IsNullOrEmpty(listId))
                            {
                                string text = DataAccess.EscapeString(playListSaveTB.Text);
                                List<PlayListNameResultSet> list = dao.GetAllResultsWhere(@" WHERE Name='" + text + "'");
                                if (list.Count > 0)
                                {
                                    listId = list[0].Id;
                                }
                            }

                            // Insert Song-PlayListName IDs into the table
                            PlayListNamesToMediaItemsResultSet plrs = new PlayListNamesToMediaItemsResultSet();
                            plrs.MediaId = songId;
                            plrs.PlayListId = listId;
                            PlayListNamesToMediaItemsDao<PlayListNamesToMediaItemsResultSet> pldao = new PlayListNamesToMediaItemsDao<PlayListNamesToMediaItemsResultSet>(plrs, plrs.TableName);
                            if (!pldao.InsertResultSet(plrs))
                            {
                                MakeErrorViewVisible();
                            }
                        }
                    }
                }
                saveBtnClickNum = 0;
                playListSaveTB.Text = String.Empty;
                playListSaveTB.Visibility = Visibility.Collapsed;
            }
            else if (playListSaveTB.Visibility == Visibility.Visible && (saveBtnClickNum >= 2))
            {
                // We clicked the button without entering anything into the textbox
                saveBtnClickNum = 0;
                playListSaveTB.Text = String.Empty;
                playListSaveTB.Visibility = Visibility.Collapsed;
            }
        }

        int numLoadClicks = 0;
        /// <summary>
        /// Event handler which responds to the Load PlayList button clicks.  Clicking the button the first time
        /// will pop up a combobox with the saved playlist names.  If the button is clicked without entering a
        /// name, the combobox is popped down.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadBtn_Click(object sender, RoutedEventArgs e)
        {
            ++numLoadClicks;

            if (numLoadClicks == 1)
            {
                savedPlayListViewModel.savedPlayListItems.Clear();

                // Get list of files to add to combobox
                PlayListNameResultSet prs = new PlayListNameResultSet();
                PlayListNameDao<PlayListNameResultSet> pdao = new PlayListNameDao<PlayListNameResultSet>(prs, prs.TableName);
                List<PlayListNameResultSet> playListResults = pdao.GetAllResults();
                if (playListResults.Count > 0)
                {
                    foreach (var item in playListResults)
                    {
                        SavedPlayListModel model = new SavedPlayListModel();
                        model.Name = item.Name;
                        savedPlayListViewModel.savedPlayListItems.Add(model);
                    }
                    savedPlayListsCB.Visibility = Visibility.Visible;
                }
            }
            else if ((savedPlayListsCB.Visibility == Visibility.Visible) && (numLoadClicks >= 2))
            {
                // The button was clicked without making a selection in the combobox
                savedPlayListsCB.Visibility = Visibility.Collapsed;
                numLoadClicks = 0;
            }
        }

        int numDeleteClicks = 0;
        /// <summary>
        /// Event handler which responds to the Delete PlayList button clicks.  Clicking the button the first time
        /// will pop up a combobox with the saved playlist names.  If the button is clicked without entering a
        /// name, the combobox is popped down.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeletePlayListBtn_Click(object sender, RoutedEventArgs e)
        {
            ++numDeleteClicks;

            if (numDeleteClicks == 1)
            {
                savedPlayListViewModel.deletePlayListItems.Clear();

                // Get list of names to add to combobox
                PlayListNameResultSet prs = new PlayListNameResultSet();
                PlayListNameDao<PlayListNameResultSet> pdao = new PlayListNameDao<PlayListNameResultSet>(prs, prs.TableName);
                List<PlayListNameResultSet> playListResults = pdao.GetAllResults();
                if (playListResults.Count > 0)
                {
                    foreach (var item in playListResults)
                    {
                        SavedPlayListModel model = new SavedPlayListModel();
                        model.Name = item.Name;
                        savedPlayListViewModel.deletePlayListItems.Add(model);
                    }
                    deletePlayListCB.Visibility = Visibility.Visible;
                }
            }
            else if ((numDeleteClicks == 2) && (deletePlayListCB.SelectedIndex > -1))
            {
                string playListName = String.Empty;
                int selectedPlayListNameIdx = deletePlayListCB.SelectedIndex;
                // Get PlayList Names
                if (selectedPlayListNameIdx > -1)
                {
                    // A selection was made.  Get the name and add the playlist items into the playlist listview
                    playListName = savedPlayListViewModel.DeletePlayListItems[selectedPlayListNameIdx].Name;
                    if (!String.IsNullOrEmpty(playListName))
                    {
                        // Just in case there's quotes in the name
                        string escapedName = DataAccess.EscapeString(playListName);

                        // Capture the ID of the playList name from the DB table
                        string nameId = String.Empty;
                        PlayListNameResultSet nameRs = new PlayListNameResultSet();
                        PlayListNameDao<PlayListNameResultSet> nameDao = new PlayListNameDao<PlayListNameResultSet>(nameRs, nameRs.TableName);
                        List<PlayListNameResultSet> nameList = nameDao.GetAllResultsWhere(" WHERE  Name='" + escapedName + "'");
                        if (nameList.Count > 0)
                        {
                            nameId = nameList[0].Id;
                        }

                        // Now we delete the rows in the bridge table that have the name ID
                        PlayListNamesToMediaItemsResultSet rs = new PlayListNamesToMediaItemsResultSet();
                        PlayListNamesToMediaItemsDao<PlayListNamesToMediaItemsResultSet> dao = new PlayListNamesToMediaItemsDao<PlayListNamesToMediaItemsResultSet>(rs, rs.TableName);
                        if (!dao.DeleteAllResultsWhere(@" WHERE PlayListId=" + nameId))
                        {
                            MakeErrorViewVisible();
                        }
                        else
                        {
                            // Now we delete the name from the Name table
                            if (!nameDao.DeleteAllResultsWhere(@" WHERE Id=" + nameId))
                            {
                                MakeErrorViewVisible();
                            }
                            else
                            {
                                // Delete the name from the DeletePlayList itemsource
                                SavedPlayListModel pModel = new SavedPlayListModel();
                                pModel.Name = playListName;
                                if (savedPlayListViewModel.deletePlayListItems.Remove(pModel))
                                {
                                    System.Diagnostics.Debug.WriteLine("Item removed from itemsource [{0}]", playListName);
                                    deletePlayListCB.SelectedItem = -1;
                                }
                            }
                        }
                    }
                }
                deletePlayListCB.SelectedIndex = -1;
                deletePlayListCB.Visibility = Visibility.Collapsed;
                numDeleteClicks = 0;
            }
            else if ((deletePlayListCB.Visibility == Visibility.Visible) && (deletePlayListCB.SelectedIndex == -1) && (numDeleteClicks >= 2))
            {
                // The button was clicked without making a selection in the combobox
                deletePlayListCB.Visibility = Visibility.Collapsed;
                numDeleteClicks = 0;
            }
        }

        bool fromSavedPlayList = false;
        /// <summary>
        /// Event handler which responds to making selections in the Save PlayList combobox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SavedPlayListCB_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            string playListName = null;
            ComboBox box = sender as ComboBox;
            if (box != null)
            {
                int selectedPlayListNameIdx = box.SelectedIndex;
                if (selectedPlayListNameIdx > -1)
                {
                    // A selection was made.  Get the name and add the playlist items into the playlist listview
                    playListName = savedPlayListViewModel.SavedPlayListItems[selectedPlayListNameIdx].Name;
                    if (!String.IsNullOrEmpty(playListName))
                    {
                        // Just in case there's quotes in the name
                        string escapedName = DataAccess.EscapeString(playListName);

                        // Get all the songs associated with this playlist name
                        PlayListViewResultSet rs = new PlayListViewResultSet();
                        PlayListViewDao<PlayListViewResultSet> dao = new PlayListViewDao<PlayListViewResultSet>(rs, rs.TableName);
                        List<PlayListViewResultSet> list = dao.GetAllResultsWhere(@" WHERE PlayListName='" + escapedName + "' ORDER BY 'Id'");

                        if (list != null)
                        {
                            // This flag is for syncing with the playlist collection changed handler
                            fromSavedPlayList = true;

                            // Go through the list returned from the DB and add them to the playlist listview itemsource
                            foreach (var item in list)
                            {
                                PlayListViewModel vm = new PlayListViewModel();
                                vm.OrderId = Convert.ToInt32(item.Id);
                                vm.ArtistName = item.Artist;
                                vm.File = item.FileName;
                                vm.Length = item.SongLength;
                                vm.Path = item.FilePath;
                                vm.Song = item.Title;
                                vm.Selected = false;
                                vm.NowPlaying = false;

                                playListItems.Add(vm);
                            }
                            playList.ItemsSource = playListItems;
                            box.Visibility = Visibility.Collapsed;
                        }
                    }
                    numLoadClicks = 0;
                    fromSavedPlayList = false;
                }
            }
        }

        /// <summary>
        /// Event handler which responds to making selections in the Delete PlayList combobox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeletePlayListCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox box = sender as ComboBox;
            if (box != null)
            {
                selectedIdx = box.SelectedIndex;
            }
        }

        /// <summary>
        /// The event handler for the Expand playlist section button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FlyoutButtonExpand_Click(object sender, RoutedEventArgs e)
        {
            mediaViewerViewModel.FlyoutExpand = !mediaViewerViewModel.FlyoutExpand;
            mediaViewerViewModel.FlyoutContract = true;
        }

        /// <summary>
        /// The event handler for the Contract playlist section button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FlyoutContractButton_Click(object sender, RoutedEventArgs e)
        {
            mediaViewerViewModel.FlyoutContract = !mediaViewerViewModel.FlyoutContract;
            mediaViewerViewModel.FlyoutExpand = true;
        }

        #endregion

        #region Media Play Event Handlers

        private void PlayBtn_Click(object sender, RoutedEventArgs e)
        {
            if (mediaPlay == null)
            {
                List<string> items = new List<string>();
                foreach (var title in playListItems)
                {
                    items.Add(title.Path + "\\" + title.Song + ".flac");
                }
                mediaPlay = new MediaPlayProcess(this, items);
            }
            Debug.WriteLine("Track Count: {0}", mediaPlay.TrackCount());
            if (!mediaPlay.IsFastForward() && !mediaPlay.IsRewind())
            {
                isPlaying = !isPlaying;
            }
            if (playList.SelectedIndex > -1)
            {
                mediaPlay.Play(isPlaying, playList.SelectedIndex);
            }
            else
            {
                mediaPlay.Play(isPlaying);
            }
        }

        private void PauseBtn_Click(object sender, RoutedEventArgs e)
        {
            isPlaying = false;
            mediaPlay.Play(false);
        }

        private void RwBtn_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            rwdButtonMultiClick.Element_MouseLeftButtonDown(sender, e);
        }

        private void RwdBtn_SingleClickAction()
        {
            if (mediaPlay.GetState() == MediaPlayProcess.MediaPlayStateEnum.MEDIA_PLAY)
            {
                mediaPlay._mediaPlayer_Backward(rwBtn, new RoutedEventArgs());
            }
        }

        private void RwdBtn_DoubleClickAction()
        {
            if (mediaPlay.GetState() == MediaPlayProcess.MediaPlayStateEnum.MEDIA_PLAY)
            {
                mediaPlay.PreviousTrack();
            }
        }

        private void StopBtn_Click(object sender, RoutedEventArgs e)
        {
            isPlaying = false;
            mediaPlay.Stop();
        }

        #endregion

        private void FfBtn_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            fFwdButtonMultiClick.Element_MouseLeftButtonDown(sender, e);
        }

        private void FastFwdBtn_Click(object sender, RoutedEventArgs e)
        {
            if (mediaPlay.GetState() == MediaPlayProcess.MediaPlayStateEnum.MEDIA_PLAY)
            {
                mediaPlay._mediaPlayer_Forward(sender, e);
            }
        }

        private void FFBtn_DoubleClickAction()
        {
            if (mediaPlay.GetState() == MediaPlayProcess.MediaPlayStateEnum.MEDIA_PLAY)
            {
                mediaPlay.NextTrack();
            }
        }

        private void FFBtn_SingleClickAction()
        {
            if (mediaPlay.GetState() == MediaPlayProcess.MediaPlayStateEnum.MEDIA_PLAY)
            {
                mediaPlay._mediaPlayer_Forward(ffBtn, new RoutedEventArgs());
            }
        }

        private void MediaDetailsControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void VolumeButton_Click(object sender, RoutedEventArgs e)
        {
            volumeControl.Visibility = ((isVolumeControlVisible = !isVolumeControlVisible) ? Visibility.Visible : Visibility.Collapsed);
        }

        private void VolumeControl_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            IntPtr data = new IntPtr();
            //System.Diagnostics.Debug.WriteLine("Volume Changed {0}", volumeControl.Volume);
            mediaPlay.MediaPlayer_ChangeVolume(data, (float)volumeControl.Volume / 50, volumeControl.Mute);
            SetVolumeControlImage();
        }


        #endregion

    }
}
