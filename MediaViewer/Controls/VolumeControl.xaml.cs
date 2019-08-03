using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace MediaViewer
{
    /// <summary>
    /// Interaction logic for VolumeControl.xaml
    /// </summary>
    public partial class VolumeControl : UserControl, INotifyPropertyChanged
    {
        private double _volume;
        private bool mouseCaptured = false;
        private bool _mute = false;

        public double Volume
        {
            get { return _volume; }
            set
            {
                _volume = value;
                OnPropertyChanged("Volume");
            }
        }

        public bool Mute
        {
            get { return _mute; }
            set
            {
                _mute = value;
                OnPropertyChanged("Mute");
            }
        }

        public VolumeControl()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void MouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed && mouseCaptured)
            {
                var x = e.GetPosition(volumeBar).X;
                var ratio = x / volumeBar.ActualWidth;
                Volume = ratio * volumeBar.Maximum;
            }
        }

        private void MouseDown(object sender, MouseButtonEventArgs e)
        {
            mouseCaptured = true;
            var x = e.GetPosition(volumeBar).X;
            var ratio = x / volumeBar.ActualWidth;
            Volume = ratio * volumeBar.Maximum;
        }

        private void MouseUp(object sender, MouseButtonEventArgs e)
        {
            mouseCaptured = false;
        }

        #region Property Changed
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private void MuteButton_Click(object sender, RoutedEventArgs e)
        {
            Mute = !Mute;
        }

        private void OuterBorder_MouseMove(object sender, MouseEventArgs e)
        {
            Point pos = e.GetPosition(this);
            // This diff is because there's a skew in the height of the control vs mouse position.
            // The height is announced as 44, but the mouse position is registered differently.
            double diffY = 9.0D;
            if (((pos.X < this.Width) && (pos.X > 0)) &&
                ((pos.Y < (this.Height - diffY)) && (pos.Y > (0 + diffY))))
            {
                outerBorder.Background = new SolidColorBrush(SystemColors.MenuHighlightColor);
            }
            else
            {
                outerBorder.Background = new SolidColorBrush(Colors.Gray);
            }
        }
    }
}
