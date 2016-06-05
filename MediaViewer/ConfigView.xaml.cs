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
        /// Constructor
        /// </summary>
        public ConfigView()
        {
            InitializeComponent();
            // Set the Data Context to the View Model
            this.DataContext = viewModel;
            // Create the DAO using the resultset and get the config items from the DB
            dao = new MediaViewConfigDao<MediaViewConfigResultSet>(rs, rs.TableName);
            configList = dao.GetAllResults();
            // Set some File Dialog properties
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Filter = "*.exe|*.exe";
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
        /// Load the view model with data obtained by the DAO in the constructor
        /// </summary>
        private void LoadConfigItems()
        {
            if (configList.Count > 0)
            {
                viewModel.VlcPath = configList[0].VlcPath;
            }
        }

        /// <summary>
        /// The Submit button Click handler.  This will send any new config items to the DB
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Submit_Button_Click(object sender, RoutedEventArgs e)
        {
            string path = VlcLocation_TextBox.Text;
            if (!string.IsNullOrEmpty(path))
            {
                rs.VlcPath = path;
                if (!dao.InsertResultSet(rs))
                {
                    Console.WriteLine("Unsuccessful Data Insert");
                }
            }
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
            if (!String.IsNullOrEmpty(fileName))
            {
                VlcLocation_TextBox.Text = fileName;
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
            Hide();
        }
    }
}
