using System;
using System.IO;
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
using Microsoft.Win32;

namespace MediaViewer
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UserControl1 : UserControl
    {
        /// <summary>
        /// File Dialog window to choose files
        /// </summary>
        private readonly OpenFileDialog openFileDialog = new OpenFileDialog();
        /// <summary>
        /// View model for file info
        /// </summary>
        private ViewModel viewModel = new ViewModel();

        /// <summary>
        /// Constructor
        /// </summary>
        public UserControl1()
        {
            InitializeComponent();

            openFileDialog.RestoreDirectory = true;

            this.DataContext = viewModel;

            openFileDialog.Filter = "*.mp3;*.flac|*.mp3;*.flac";
        }

        /// <summary>
        /// Public method to load the file info into the view model
        /// </summary>
        /// <param name="fileName"></param>
        public void LoadViewModel(string fileName)
        {
            if (viewModel != null)
            {
                viewModel.LoadFile(fileName);
            }
        }

        /// <summary>
        /// The Browse button click event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void browse_ButtonClick(object sender, RoutedEventArgs e)
        {
            if (openFileDialog.ShowDialog() != true)
            {
                e.Handled = false;
                return;
            }
            string fileName = openFileDialog.FileName;

            if (!String.IsNullOrEmpty(fileName))
            {
                viewModel.LoadFile(fileName);
            }
            e.Handled = true;
        }
    }
}
