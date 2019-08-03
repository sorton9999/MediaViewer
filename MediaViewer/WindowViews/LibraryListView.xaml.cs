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
using System.Windows.Shapes;

using DataAccessLib;


namespace MediaViewer
{
    /// <summary>
    /// Interaction logic for LibraryListView.xaml
    /// </summary>
    public partial class LibraryListView : Window
    {
        System.Collections.ObjectModel.ObservableCollection<string> libraryPathList =
            new System.Collections.ObjectModel.ObservableCollection<string>();
        static SearchPathResultSet rs = new SearchPathResultSet();
        SearchPathDao<SearchPathResultSet> data = new SearchPathDao<SearchPathResultSet>(rs, rs.TableName);

        MainWindow _parent = null;

        private bool dataChanged = false;

        public LibraryListView(MainWindow parent)
        {
            InitializeComponent();
            _parent = parent;
            this.libraryPathListView.ItemsSource = libraryPathList;
            LoadLibraryList();
        }

        public bool DataChanged
        {
            get { return dataChanged; }
            private set
            {
                dataChanged = value;
                _parent.DataChanged = value;
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            using (var fbd = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (fbd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                {
                    e.Handled = false;
                    return;
                }
                string dirName = fbd.SelectedPath;
                try
                {
                    libraryPathList.Add(dirName);
                    DataChanged = true;
                }
                catch (Exception)
                {
                }
            }
            e.Handled = true;
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (libraryPathListView.SelectedIndex >= 0)
            {
                try
                {
                    libraryPathList.Remove((string)libraryPathListView.SelectedItem);
                    DataChanged = true;
                }
                catch (Exception)
                {
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            CommitLibraryPaths();
            dataChanged = false;
            this.Close();
        }

        private void LoadLibraryList()
        {
            // Load Library Path list from DB
            List<SearchPathResultSet> resList = data.GetAllResults();
            foreach (var item in resList)
            {
                libraryPathList.Add(item.DirPath);
            }
        }

        private void CommitLibraryPaths()
        {
            // Send Library Path list contents to the DB. but only if
            // new data was added
            if (!dataChanged)
            {
                return;
            }
            data.ClearTable();
            foreach (var item in libraryPathList)
            {
                rs.DirPath = item;
                if (!data.InsertResultSet(rs))
                {
                    Console.WriteLine("[[[ Insert Error ]]] -- {0}", item);
                }
            }
        }
    }
}
