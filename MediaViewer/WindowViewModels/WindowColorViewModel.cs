using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MediaViewer
{
    public class WindowColorViewModel : ViewModelBase
    {
        Color _topBackgroundColor;
        Color _bottomBackgroundColor;
        Color _lightSolidColor;
        Color _darkSolidColor;
        Color _lightButtonSolidColor;
        Color _darkButtonSolidColor;
        Color _activeButtonSolidColor;

        SolidColorBrush _lightSolidColorBrush;
        SolidColorBrush _darkSolidColorBrush;
        SolidColorBrush _lightButtonSolidColorBrush;
        SolidColorBrush _darkButtonSolidColorBrush;
        SolidColorBrush _activeButtonSolidColorBrush;

        LinearGradientBrush _gradientBrush;
        GradientStopCollection gradCollection = new GradientStopCollection();


        public Color WindowTopBackgroundColor
        {
            get { return _topBackgroundColor; }
            set
            {
                _topBackgroundColor = value;
                NotifyPropertyChanged("WindowTopBackgroundColor");
            }
        }

        public Color WindowBottomBackgroundColor
        {
            get { return _bottomBackgroundColor; }
            set
            {
                _bottomBackgroundColor = value;
                NotifyPropertyChanged("WindowBottomBackgroundColor");
            }
        }

        public Color LightSolidColor
        {
            get { return _lightSolidColor; }
            set
            {
                _lightSolidColor = value;
                NotifyPropertyChanged("LightSolidColor");
            }
        }

        public Color DarkSolidColor
        {
            get { return _darkSolidColor; }
            set
            {
                _darkSolidColor = value;
                NotifyPropertyChanged("DarkSolidColor");
            }
        }

        public Color LightButtonSolidColor
        {
            get { return _lightButtonSolidColor; }
            set
            {
                _lightButtonSolidColor = value;
                NotifyPropertyChanged("LightButtonSolidColor");
            }
        }

        public Color DarkButtonSolidColor
        {
            get { return _darkButtonSolidColor; }
            set
            {
                _darkButtonSolidColor = value;
                NotifyPropertyChanged("DarkButtonSolidColor");
            }
        }

        public Color ActiveButtonSolidColor
        {
            get { return _activeButtonSolidColor; }
            set
            {
                _activeButtonSolidColor = value;
                NotifyPropertyChanged("ActiveButtonSolidColor");
            }
        }


        public SolidColorBrush LightSolidColorBrush
        {
            get { return _lightSolidColorBrush ?? new SolidColorBrush(_lightSolidColor); }
            set
            {
                _lightSolidColorBrush = value;
                NotifyPropertyChanged("LightSolidColorBrush");
            }
        }

        public SolidColorBrush DarkSolidColorBrush
        {
            get { return _darkSolidColorBrush ?? new SolidColorBrush(_darkSolidColor); }
            set
            {
                _darkSolidColorBrush = value;
                NotifyPropertyChanged("DarkSolidColorBrush");
            }
        }

        public SolidColorBrush LightButtonSolidColorBrush
        {
            get { return _lightButtonSolidColorBrush ?? new SolidColorBrush(_lightButtonSolidColor); }
            set
            {
                _lightButtonSolidColorBrush = value;
                NotifyPropertyChanged("LightButtonSolidColorBrush");
            }
        }

        public SolidColorBrush DarkButtonSolidColorBrush
        {
            get { return _darkButtonSolidColorBrush ?? new SolidColorBrush(_darkButtonSolidColor); }
            set
            {
                _darkButtonSolidColorBrush = value;
                NotifyPropertyChanged("DarkButtonSolidColorBrush");
            }
        }

        public SolidColorBrush ActiveButtonSolidColorBrush
        {
            get { return _activeButtonSolidColorBrush ?? new SolidColorBrush(_activeButtonSolidColor); }
            set
            {
                _activeButtonSolidColorBrush = value;
                NotifyPropertyChanged("ActiveButtonSolidColorBrush");
            }
        }

        public LinearGradientBrush WindowGradientBrush
        {
            get 
            {
                gradCollection.Add(new GradientStop(_topBackgroundColor, 0));
                gradCollection.Add(new GradientStop(_bottomBackgroundColor, 1));
                return _gradientBrush ?? new LinearGradientBrush(gradCollection, new System.Windows.Point(0.5, 1.0), new System.Windows.Point(0.5, 0));
            }
            set
            {
                _gradientBrush = value;
                NotifyPropertyChanged("WindowGradientBrush");
            }
        }
    }
}
