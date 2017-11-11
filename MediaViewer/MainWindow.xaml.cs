using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Threading;
using DataAccessLib;
using System.Windows.Controls;
using System.Reflection;
using System.IO;
using System.Threading.Tasks;
using System.ComponentModel;

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
                            "*.wav"
                        };

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

        private System.Threading.Timer setupTimer;
        private bool isVlcPathSet = false;

        private Object syncLock = new object();

        /// <summary>
        /// The length of song time string
        /// </summary>
        private object timeLengthStr;
        private int songIndicatorPercent;

        public static Action stopMusicPlaying;


        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            // Register for the database data operation error events
            data.InsertErrorEvent += data_InsertErrorEventHandler;
            // Register for the item tree control selected item event
            TreeViewModel.TreeViewItemViewModel.OnItemSelected += TreeViewItemViewModel_OnItemSelected;

            MediaPlayProcess.AddPlayListEvent += MediaPlayProcess_AddPlayListEvent;
            //PlayListItemsModel playListModel = new PlayListItemsModel();
            //playListView.DataContext = playListModel;

            // Set the item source for the DB operation error window to the error string list
            errorView.ErrorList.ItemsSource = errorList;
            // Config View send parent in
            //configView = new ConfigView(this);
            // Set the Max value from ProgressBar Maximum value
            progressBarMax = 100.0;

            setupTimer = new Timer(new TimerCallback(OnSetup), null, 0, 200);

            stopMusicPlaying = new Action(StopMusic);

            // Set up media player events
            myControl.MediaPlayer.Backward += MediaPlayer_Backward;
            myControl.MediaPlayer.Buffering += MediaPlayer_Buffering;
            myControl.MediaPlayer.EncounteredError += MediaPlayer_EncounteredError;
            myControl.MediaPlayer.EndReached += MediaPlayer_EndReached;
            myControl.MediaPlayer.Forward += MediaPlayer_Forward;
            myControl.MediaPlayer.LengthChanged += MediaPlayer_LengthChanged;
            myControl.MediaPlayer.MediaChanged += MediaPlayer_MediaChanged;
            myControl.MediaPlayer.Opening += MediaPlayer_Opening;
            myControl.MediaPlayer.PausableChanged += MediaPlayer_PausableChanged;
            myControl.MediaPlayer.Paused += MediaPlayer_Paused;
            myControl.MediaPlayer.Playing += MediaPlayer_Playing;
            myControl.MediaPlayer.PositionChanged += MediaPlayer_PositionChanged;
            myControl.MediaPlayer.ScrambledChanged += MediaPlayer_ScrambledChanged;
            myControl.MediaPlayer.SeekableChanged += MediaPlayer_SeekableChanged;
            myControl.MediaPlayer.SnapshotTaken += MediaPlayer_SnapshotTaken;
            myControl.MediaPlayer.Stopped += MediaPlayer_Stopped;
            myControl.MediaPlayer.TimeChanged += MediaPlayer_TimeChanged;
            myControl.MediaPlayer.TitleChanged += MediaPlayer_TitleChanged;
            myControl.MediaPlayer.VideoOutChanged += MediaPlayer_VideoOutChanged;

            myControl.MediaPlayer.VlcLibDirectoryNeeded += MediaPlayer_VlcLibDirectoryNeeded;
            myControl.MediaPlayer.EndInit();

            BindingOperations.EnableCollectionSynchronization(errorList, syncLock);
        }

        private void StopMusic()
        {
            myControl.MediaPlayer.Stop();
        }

        private void MediaPlayer_VideoOutChanged(object sender, Vlc.DotNet.Core.VlcMediaPlayerVideoOutChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("MediaPlayer_VideoOutChanged");
        }

        private void MediaPlayer_TitleChanged(object sender, Vlc.DotNet.Core.VlcMediaPlayerTitleChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("MediaPlayer_TitleChanged");
        }

        private void MediaPlayer_TimeChanged(object sender, Vlc.DotNet.Core.VlcMediaPlayerTimeChangedEventArgs e)
        {
            long timeDigit = e.NewTime / 1000L;
            string secs = (timeDigit % 60).ToString("00");
            string mins = ((timeDigit / 60) % 60).ToString("00");
            string hrs = ((timeDigit / 3600) % 60).ToString("00");
            timeLengthStr = hrs + ":" + mins + ":" + secs;
            CheckAndInvoke(new Action(AdvanceSongTimeLabel));
        }

        private void MediaPlayer_Stopped(object sender, Vlc.DotNet.Core.VlcMediaPlayerStoppedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("MediaPlayer_Stopped");
        }

        private void MediaPlayer_SnapshotTaken(object sender, Vlc.DotNet.Core.VlcMediaPlayerSnapshotTakenEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("MediaPlayer_SnapshotTaken");
        }

        private void MediaPlayer_SeekableChanged(object sender, Vlc.DotNet.Core.VlcMediaPlayerSeekableChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("MediaPlayer_SeekableChanged");
        }

        private void MediaPlayer_ScrambledChanged(object sender, Vlc.DotNet.Core.VlcMediaPlayerScrambledChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("MediaPlayer_ScrambledChanged");
        }

        private void MediaPlayer_PositionChanged(object sender, Vlc.DotNet.Core.VlcMediaPlayerPositionChangedEventArgs e)
        {
            songIndicatorPercent = (int)(e.NewPosition * 100.0);
            CheckAndInvoke(new Action(AdvanceNormalSliderTime));
        }

        private void MediaPlayer_Playing(object sender, Vlc.DotNet.Core.VlcMediaPlayerPlayingEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("MediaPlayer_Playing");
        }

        private void MediaPlayer_Paused(object sender, Vlc.DotNet.Core.VlcMediaPlayerPausedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("MediaPlayer_Paused");
        }

        private void MediaPlayer_PausableChanged(object sender, Vlc.DotNet.Core.VlcMediaPlayerPausableChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("MediaPlayer_PausableChanged");
        }

        private void MediaPlayer_Opening(object sender, Vlc.DotNet.Core.VlcMediaPlayerOpeningEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("MediaPlayer_Opening");
        }

        private void MediaPlayer_MediaChanged(object sender, Vlc.DotNet.Core.VlcMediaPlayerMediaChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("MediaPlayer_MediaChanged");
        }

        private void MediaPlayer_EncounteredError(object sender, Vlc.DotNet.Core.VlcMediaPlayerEncounteredErrorEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("MediaPlayer_EncounteredError");
        }

        private void MediaPlayer_Buffering(object sender, Vlc.DotNet.Core.VlcMediaPlayerBufferingEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("MediaPlayer_Buffering");
        }

        private void MediaPlayer_Backward(object sender, Vlc.DotNet.Core.VlcMediaPlayerBackwardEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("MediaPlayer_Backward");
        }

        private void MediaPlayer_LengthChanged(object sender, Vlc.DotNet.Core.VlcMediaPlayerLengthChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("MediaPlayer_LengthChanged");
        }

        private void MediaPlayer_Forward(object sender, Vlc.DotNet.Core.VlcMediaPlayerForwardEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("MediaPlayer_Forward");
        }

        private void MediaPlayer_EndReached(object sender, Vlc.DotNet.Core.VlcMediaPlayerEndReachedEventArgs e)
        {
            String song = MediaPlayProcess.PlayNext();
            if (!String.IsNullOrEmpty(song))
            {
                myControl.MediaPlayer.Play(new FileInfo(song));
            }
        }

        private void MediaPlayProcess_AddPlayListEvent(string song, string length, bool playNow)
        {
            // We only need the song name out of this path.
            // So cut out everything up to and including the last \ and
            // cut off the file extension.
            int len = song.Length;
            int slashIdx = song.LastIndexOf('\\');
            int dotIdx = song.LastIndexOf('.');
            string title = song.Substring((slashIdx + 1), (len - (slashIdx + 1) - (len - dotIdx)));
            // add to the list view the title of the song w/o the path and extension and the song length
            PlayListItemsModel temp = new PlayListItemsModel() { Song = title, Length = length };
            playListView.Items.Add(temp);
            // Depending on what the right-click selection is, play the song
            if (playNow)
            {
                myControl.MediaPlayer.Play(new FileInfo(song));
            }
        }

        /// <summary>
        /// Timer callback which checks for configuration item set.
        /// </summary>
        /// <param name="state"></param>
        private void OnSetup(object state)
        {
            isVlcPathSet = ((configView != null) && configView.IsPathConfigured());
        }

        private void MediaPlayer_VlcLibDirectoryNeeded(object sender, Vlc.DotNet.Forms.VlcLibDirectoryNeededEventArgs e)
        {
            /*
            var currentAssembly = Assembly.GetEntryAssembly();
            var currentDirectory = new FileInfo(@"C:\\Users\\Orton\\src\\lib\\Vlc.DotNet - master\\lib").DirectoryName;
            if (currentDirectory == null)
            {
                return;
            }
            if (AssemblyName.GetAssemblyName(currentAssembly.Location).ProcessorArchitecture == ProcessorArchitecture.X86)
            {
                e.VlcLibDirectory = new DirectoryInfo(currentDirectory + @"\lib\x86\");
                //e.VlcLibDirectory = new DirectoryInfo(Path.Combine(currentDirectory, @"\lib\x86\"));
            }
            else
            {
                e.VlcLibDirectory = new DirectoryInfo(currentDirectory + @"\lib\x64\");
                //e.VlcLibDirectory = new DirectoryInfo(Path.Combine(currentDirectory, @"\lib\x64\"));
            }
            */
            //e.VlcLibDirectory = new DirectoryInfo(@"C:\Users\Orton\src\lib\Vlc.DotNet-master\lib\x64");
            bool alreadyConfigured = false;
            if (configView == null)
            {
                configView = new ConfigView(this);
            }
            if (configView.IsPathConfigured())
            {
                e.VlcLibDirectory = new DirectoryInfo(ConfigView.ConfigViewModel.VlcPath);
                alreadyConfigured = true;
            }
            else
            {
                configView.ShowDialog();
            }

            if (!alreadyConfigured && isVlcPathSet)
            {
                e.VlcLibDirectory = new DirectoryInfo(ConfigView.ConfigViewModel.VlcPath);
            }
            //e.VlcLibDirectory = new DirectoryInfo(@"C:\Program Files (x86)\VideoLAN\VLC");
            //e.VlcLibDirectory = new DirectoryInfo(@"C:\Users\Orton\src\lib\x64");
        }

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

        public ProgressBar GetProgressBar
        {
            get
            {
                return (ProgressBar)Media_Button.Template.FindName("WorkProgressBar", Media_Button);
            }
        }

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
                    // Load the view model for the detaile control using the path and file name
                    mediaDetailsControl.LoadViewModel(title.FilePath + "\\" + title.FileName);
                }
            }
        }

        /// <summary>
        /// The handler for the database operation error events
        /// </summary>
        /// <param name="message">The error message</param>
        void data_InsertErrorEventHandler(string message)
        {
            // Add the incoming error message to the error message list
            errorList.Add(message);
        }

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
            if (GetProgressBar.Visibility != System.Windows.Visibility.Visible)
            {
                GetProgressBar.Visibility = System.Windows.Visibility.Visible;
            }
            GetProgressBar.Value = 0.0;
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

            // Cycle through all the user set directories
            foreach (var dir in dirList)
            {
                // Cycle through each file type defined in the filerList class var.
                foreach (var filterType in filterList)
                {
                    // Obtain the directory info for the current directory
                    System.IO.DirectoryInfo fileDirInfo = new System.IO.DirectoryInfo(dir.DirPath);
                    if (fileDirInfo != null)
                    {
                        List<System.IO.FileInfo> fileList = fileDirInfo.EnumerateFiles(filterType, System.IO.SearchOption.AllDirectories).ToList();
                        int val = fileList.Count();
                        //progressBarStep = (progressBarMax - progressBarValue) / ((filterList.Count + val) / dirList.Count);
                        progressBarStep = (progressBarMax - progressBarValue) / val;

                        // Filter out the desired files in the directory and iterate through each one
                        foreach (var file in fileList)
                        {
                            // Grab the directory path and file name to grab the imbedded data in the media file.
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
                                rs.SongLength = mediaInfo.Properties.Duration.ToString(@"mm\:ss");
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

                            // Advance progress bar a bit after processing this file
                            CheckAndInvoke(new Action(AdvanceProgressBar));
                        }

                    }
                    progressBarStep = (progressBarMax / dirList.Count) / filterList.Count;
                    CheckAndInvoke(new Action(AdvanceProgressBar));
                }
                progressBarStep = progressBarMax / dirList.Count;
                CheckAndInvoke(new Action(AdvanceProgressBar));
            }
            // Advance progress bar to max if needed
            CheckAndInvoke(new Action(ProgressBarStop));

            // A small notification the work has finished
            MessageBox.Show("Media Import Is Complete");

            // Reload the tree once all work has finished and the above dialog has been acknowledged.
            CheckAndInvoke(libraryTreeControl.libraryTreeUpdateAction);
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
        /// Advance the media search progress bar as work is being done
        /// </summary>
        private void AdvanceProgressBar()
        {
            GetProgressBar.Value += progressBarStep;
            progressBarValue = GetProgressBar.Value;
        }

        /// <summary>
        /// Advance the progress bar to its maximum value to indicate completion
        /// </summary>
        private void ProgressBarStop()
        {
            GetProgressBar.Value = GetProgressBar.Maximum;
        }

        /// <summary>
        /// Ensure the given value is within the range Min - Max
        /// </summary>
        /// <param name="value">The incoming value</param>
        /// <param name="step">The value to step the incoming value to</param>
        /// <param name="min">The minimum the value should equal</param>
        /// <param name="max">The maximum the value should equal</param>
        /// <returns></returns>
        double EnsureStepRange(double value, double step, double min, double max)
        {
            return Math.Min(Math.Max(value + step, min), max);
        }

        /// <summary>
        /// Advance the song label
        /// </summary>
        private void AdvanceSongTimeLabel()
        {
            Time_Label.Content = timeLengthStr;
        }

        /// <summary>
        /// Advance the song indicator during normal playback
        /// </summary>
        private void AdvanceNormalSliderTime()
        {
            progressSlider.Value = songIndicatorPercent;
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

        private void OnPauseButtonClick(object sender, RoutedEventArgs e)
        {
            myControl.MediaPlayer.Pause();
        }

        private void OnPlayButtonClick(object sender, RoutedEventArgs e)
        {
            if (myControl.MediaPlayer.Rate > 1.0F)
            {
                myControl.MediaPlayer.Rate = 1.0F;
                return;
            }
            if (myControl.MediaPlayer.IsPlaying)
            {
                return;
            }
            if (myControl.MediaPlayer.State == Vlc.DotNet.Core.Interops.Signatures.MediaStates.Paused)
            {
                myControl.MediaPlayer.Pause();
                return;
            }
            String fileStr = MediaPlayProcess.PlayNext();
            if (!String.IsNullOrEmpty(fileStr))
            {
                myControl.MediaPlayer.Play(new System.IO.FileInfo(fileStr));
            }
        }

        private void OnStopButtonClick(object sender, RoutedEventArgs e)
        {
            myControl.MediaPlayer.Stop();
        }

        private void OnFastForwardButtonClick(object sender, RoutedEventArgs e)
        {
            myControl.MediaPlayer.Rate = 2.0F;
        }

        private void OnRewindButtonClick(object sender, RoutedEventArgs e)
        {
            myControl.MediaPlayer.Rate = -2.0F;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            myControl.MediaPlayer.Stop();
            Exiting = true;
            errorView.Close();
            configView.Close();
            if (libView != null)
                libView.Close();
        }
    }
}
