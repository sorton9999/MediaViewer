using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer
{
    public class ConfigItemsViewModel : ViewModelBase
    {
        ConfigItemsModel model = new ConfigItemsModel();

        public string VlcPath
        {
            get { return model.VlcPath; }
            set
            {
                if (model.VlcPath != value)
                {
                    model.VlcPath = value;
                    NotifyPropertyChanged("VlcPath");
                }
            }
        }
    }
}
