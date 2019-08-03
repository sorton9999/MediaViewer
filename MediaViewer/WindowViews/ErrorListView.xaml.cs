using System.Windows;

namespace MediaViewer
{
    /// <summary>
    /// Interaction logic for ErrorListView.xaml
    /// </summary>
    public partial class ErrorListView : Window
    {
        public ErrorListView()
        {
            InitializeComponent();
        }

        private void ListViewLoaded(object sender, RoutedEventArgs e)
        {

        }

        private void CloseView_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = System.Windows.Visibility.Hidden;
        }
    }
}
