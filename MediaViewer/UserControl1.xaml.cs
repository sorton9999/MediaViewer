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
        private readonly OpenFileDialog openFileDialog = new OpenFileDialog();
        private ViewModel viewModel = new ViewModel();

        public UserControl1()
        {
            InitializeComponent();

            openFileDialog.RestoreDirectory = true;

            this.DataContext = viewModel;

            openFileDialog.Filter = "*.mp3;*.flac|*.mp3;*.flac";
        }

        public void LoadViewModel(string fileName)
        {
            if (viewModel != null)
            {
                viewModel.LoadFile(fileName);
            }
        }

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
