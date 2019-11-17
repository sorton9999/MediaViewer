using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace MediaViewer
{
    public class MediaViewerViewModel : ViewModelBase
    {
        private string imageFile = String.Empty;
        private int volume = 0;
        private bool flyoutExpand = true;
        private bool flyoutContract = false;

        public bool FlyoutExpand
        {
            get { return flyoutExpand; }
            set
            {
                flyoutExpand = value;
                NotifyPropertyChanged("FlyoutExpand");
            }
        }

        public bool FlyoutContract
        {
            get { return flyoutContract; }
            set
            {
                flyoutContract = value;
                NotifyPropertyChanged("FlyoutContract");
            }
        }

        public string ImageFile
        {
            get { return imageFile; }
            set
            {
                imageFile = value;
                NotifyPropertyChanged("ImageFile");
            }
        }

        public int Volume
        {
            get { return volume; }
            set
            {
                volume = value;
                NotifyPropertyChanged("Volume");
            }
        }

    }


    public class RequestImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                string name = (string)value;

                return new BitmapImage(new Uri(name, UriKind.Relative));
            }
            else return null;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    class TestValueConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // Validation of parameters goes here...

            try
            {
                var type = (int)values[0];
                var image1 = values[1];
                var image2 = values[2];
                var image3 = values[3];
                var image4 = values[4];

                if (type <= 0)
                {
                    return image4;
                }

                if (type < 25)
                {
                    return image1;
                }
                else if (type > 25 && type < 60)
                {
                    return image2;
                }
                else
                {
                    return image3;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Boolean && (bool)value)
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Visibility && (Visibility)value == Visibility.Visible)
            {
                return true;
            }
            return false;
        }
    }
}
