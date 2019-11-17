using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer
{
    public class SavedPlayListViewModel : ViewModelBase
    {
        SavedPlayListModel saveModel = new SavedPlayListModel();

        SavedPlayListModel deleteModel = new SavedPlayListModel();

        public readonly ObservableCollection<SavedPlayListModel> savedPlayListItems = new ObservableCollection<SavedPlayListModel>();

        public readonly ObservableCollection<SavedPlayListModel> deletePlayListItems = new ObservableCollection<SavedPlayListModel>();

        public ObservableCollection<SavedPlayListModel> SavedPlayListItems
        {
            get { return savedPlayListItems; }
        }

        public ObservableCollection<SavedPlayListModel> DeletePlayListItems
        {
            get { return deletePlayListItems; }
        }

        public string FileName
        {
            get { return saveModel.Name; }
            set
            {
                saveModel.Name = value;
                NotifyPropertyChanged("FileName");
            }
        }

        public string DeleteFileName
        {
            get { return deleteModel.Name; }
            set
            {
                deleteModel.Name = value;
                NotifyPropertyChanged("DeleteFileName");
            }
        }
    }
}
