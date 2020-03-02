using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MediaViewer
{
    public class WindowColorLoader
    {
        ConfigurationFileReader _conf = null;
        WindowColorViewModel _colorView = new WindowColorViewModel();

        public WindowColorLoader(ConfigurationFileReader conf)
        {
            _conf = conf;
        }

        public WindowColorViewModel ColorView
        {
            get { return _colorView; }
        }

        public void LoadColors()
        {
            try
            {
                Dictionary<string, string> items = _conf.ConfigItems["WindowColors"];
                if (items != null)
                {
                    Color color = FromName(items["ButtonSolidColor"]);
                    _colorView.LightButtonSolidColor = color;

                    color = FromName(items["TopWindowBackgroundColor"]);
                    _colorView.WindowTopBackgroundColor = color;

                    color = FromName(items["BottomWindowBackgroundColor"]);
                    _colorView.WindowBottomBackgroundColor = color;

                    color = FromName(items["LightBackgroundColor"]);
                    _colorView.LightSolidColor = color;

                    color = FromName(items["DarkBackgroundColor"]);
                    _colorView.DarkSolidColor = color;

                    color = FromName(items["ActiveButtonSolidColor"]);
                    _colorView.ActiveButtonSolidColor = color;
                }
            }
            catch (Exception e)
            {

            }
        }

        public static Color FromName(String name)
        {
            var color_props = typeof(Colors).GetProperties();
            foreach (var c in color_props)
                if (name.Equals(c.Name, StringComparison.OrdinalIgnoreCase))
                    return (Color)c.GetValue(new Color(), null);
            return Colors.Transparent;
        }


    }
}
