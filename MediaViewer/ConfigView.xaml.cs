using System;
using System.Windows;
using System.Data;
using System.Collections.Generic;
using System.Windows.Controls;
using Microsoft.Win32;

using DataAccessLib;

namespace MediaViewer
{
    /// <summary>
    /// Interaction logic for ConfigView.xaml
    /// </summary>
    public partial class ConfigView : Window
    {
        /// <summary>
        /// A default location path for the VLC media player
        /// </summary>
        public const string defaultVlcPath = "C:\\Program Files\\VideoLAN\\VLC\\";

        /// <summary>
        /// The view model of configuration items to set
        /// </summary>
        public static ConfigItemsViewModel viewModel = new ConfigItemsViewModel();

        /// <summary>
        /// The DB access object
        /// </summary>
        private DataAccess db = new DataAccess("MediaDB");

        /// <summary>
        /// The list of configuration items obtained from the DB
        /// </summary>
        private List<MediaViewConfigResultSet> configList = new List<MediaViewConfigResultSet>();

        /// <summary>
        /// The DB resultset to get and insert values
        /// </summary>
        private MediaViewConfigResultSet rs = new MediaViewConfigResultSet();

        /// <summary>
        /// The data access object associated with the MediaViewConfig table
        /// </summary>
        private MediaViewConfigDao<MediaViewConfigResultSet> dao;

        /// <summary>
        /// A file dialog window used to choose the location of the VLC application
        /// </summary>
        private readonly OpenFileDialog openFileDialog = new OpenFileDialog();

        /// <summary>
        /// Main Window parent set in the constructor
        /// </summary>
        private MainWindow parent;


        /// <summary>
        /// Constructor
        /// </summary>
        public ConfigView(MainWindow _parent)
        {
            InitializeComponent();
            parent = _parent;
            // Set the Data Context to the View Model
            this.DataContext = viewModel;
            // Create the DAO using the resultset and get the config items from the DB
            dao = new MediaViewConfigDao<MediaViewConfigResultSet>(rs, rs.TableName);
            configList = dao.GetAllResults();
            // Set some File Dialog properties
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Filter = "*.exe|*.exe";
            openFileDialog.FileName = "IGNORE";
            openFileDialog.CheckPathExists = true;
            openFileDialog.ShowReadOnly = false;
            openFileDialog.ReadOnlyChecked = true;
            openFileDialog.CheckFileExists = false;
            openFileDialog.ValidateNames = false;


            // Load up the view model
            LoadConfigItems();
        }

        /// <summary>
        /// View Model Property
        /// </summary>
        public static ConfigItemsViewModel ConfigViewModel
        {
            get { return viewModel; }
            private set { viewModel = value; }
        }

        /// <summary>
        /// Returns if the VLC configuration path has been set.  If it's been set,
        /// the database should have it stored.
        /// </summary>
        /// <returns>Whether or not the path has been stored in the DB and configured</returns>
        public bool IsPathConfigured()
        {
            if (configList == null)
            {
                return false;
            }
            string path = configList[0].VlcPath;
            if (String.IsNullOrEmpty(path))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Load the view model with data obtained by the DAO in the constructor
        /// </summary>
        private void LoadConfigItems()
        {
            if (configList.Count > 0 && !String.IsNullOrEmpty(configList[0].VlcPath))
            {
                viewModel.VlcPath = configList[0].VlcPath;
            }
            else
            {
                viewModel.VlcPath = defaultVlcPath;
                VlcLocation_TextBox.Text = defaultVlcPath;
                Submit_Button_Click(null, null);
            }
        }

        /// <summary>
        /// The Submit button Click handler.  This will send any new config items to the DB
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Submit_Button_Click(object sender, RoutedEventArgs e)
        {
            // Clear any other entries out first
            dao.ClearTable();
            // Grab the entry and insert into DB
            string path = VlcLocation_TextBox.Text;
            if (!string.IsNullOrEmpty(path))
            {
                rs.VlcPath = path;
                if (!dao.InsertResultSet(rs))
                {
                    Console.WriteLine("Unsuccessful Data Insert");
                    e.Handled = false;
                }
            }
            e.Handled = true;
        }

        /// <summary>
        /// The Browse button Click handler.  This opens the File Dialog and sets its
        /// choice into the VLC Location textbox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Browse_Button_Click(object sender, RoutedEventArgs e)
        {
            if (openFileDialog.ShowDialog() != true)
            {
                e.Handled = false;
                return;
            }
            string fileName = openFileDialog.FileName;
            int lastIdx = fileName.LastIndexOf('\\');
            string dirPath = fileName.Substring(0, (lastIdx));
            if (!String.IsNullOrEmpty(dirPath))
            {
                VlcLocation_TextBox.Text = dirPath;
            }
            e.Handled = true;
        }

        /// <summary>
        /// The window closing event handler which will just Hide the window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void configViewWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!parent.Exiting)
                e.Cancel = true;
            else
                e.Cancel = false;
            Hide();
        }
    }
}
