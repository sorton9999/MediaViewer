using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer
{
    public class PlayListViewModel : ViewModelBase
    {
        private string song = String.Empty;
        private string path = String.Empty;
        private string length = String.Empty;
        private string file = String.Empty;
        private bool selected = false;

        public string Song
        {
            get { return song; }
            set
            {
                song = value;
                NotifyPropertyChanged("Song");
            }
        }

        public string Path
        {
            get { return path; }
            set
            {
                path = value;
                NotifyPropertyChanged("Path");
            }
        }

        public string Length
        {
            get { return length; }
            set
            {
                length = value;
                NotifyPropertyChanged("Length");
            }
        }

        public string File
        {
            get { return file; }
            set
            {
                file = value;
                NotifyPropertyChanged("File");
            }
        }

        public bool Selected
        {
            get { return selected; }
            set
            {
                selected = value;
                NotifyPropertyChanged("Selected");
            }
        }
    }
}
