using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TagLib.Id3v2;
using TagLib.Mpeg;

namespace MediaViewer
{
    public class ViewModel : ViewModelBase
    {
        Model fileModel = new Model();
        public string FileName
        {
            get { return fileModel.FullFilePath; }
            set
            {
                if (fileModel.FullFilePath != value)
                {
                    fileModel.FullFilePath = value;
                    NotifyPropertyChanged("FileName");
                }
            }
        }
        public string Title
        {
            get { return fileModel.Title; }
            set
            {
                if (fileModel.Title != value)
                {
                    fileModel.Title = value;
                    NotifyPropertyChanged("Title");
                }
            }
        }
        public string Artist
        {
            get { return fileModel.Artist; }
            set
            {
                if (fileModel.Artist != value)
                {
                    fileModel.Artist = value;
                    NotifyPropertyChanged("Artist");
                }
            }
        }
        public string Album
        {
            get { return fileModel.Album; }
            set
            {
                if (fileModel.Album != value)
                {
                    fileModel.Album = value;
                    NotifyPropertyChanged("Album");
                }
            }
        }
        public string Year
        {
            get { return fileModel.Year; }
            set
            {
                if (fileModel.Year != value)
                {
                    fileModel.Year = value;
                    NotifyPropertyChanged("Year");
                }
            }
        }
        public string Comment
        {
            get { return fileModel.Comment; }
            set
            {
                if (fileModel.Comment != value)
                {
                    fileModel.Comment = value;
                    NotifyPropertyChanged("Comment");
                }
            }
        }
        public string Genre
        {
            get { return fileModel.Genre; }
            set
            {
                if (fileModel.Genre != value)
                {
                    fileModel.Genre = value;
                    NotifyPropertyChanged("Genre");
                }
            }
        }

        public string Length
        {
            get { return fileModel.Length; }
            set
            {
                if (fileModel.Length != value)
                {
                    fileModel.Length = value;
                    NotifyPropertyChanged("Length");
                }
            }
        }

        public string TotalPlayTime
        {
            get { return fileModel.TotalPlayTime; }
            set
            {
                if (fileModel.TotalPlayTime != value)
                {
                    fileModel.TotalPlayTime = value;
                    NotifyPropertyChanged("TotalPlayTime");
                }
            }
        }

        public BitmapImage AlbumArt
        {
            get { return fileModel.AlbumArt; }
            set
            {
                if (fileModel.AlbumArt != value)
                {
                    fileModel.AlbumArt = value;
                    NotifyPropertyChanged("AlbumArt");
                }
            }
        }

        public void LoadFile(string fileName)
        {
            TagLib.File file = null;
            try
            {
                file = TagLib.File.Create(fileName);
            }
            catch (TagLib.UnsupportedFormatException)
            {
                Console.WriteLine("[[[EXCEPTION]]] -- Unsupported File: " + fileName);
            }
            catch (Exception e)
            {
                Console.WriteLine("[[[EXCEPTION]]] -- File Exception: " + e.Message);
            }

            if (file != null)
            {
                FileName = fileName;
                Title = file.Tag.Title;
                Album = file.Tag.Album;
                Artist = file.Tag.FirstPerformer;
                Year = file.Tag.Year.ToString();
                Comment = file.Tag.Comment;
                Genre = file.Tag.FirstGenre;
                Length = MainWindow.ComputeSongLength(file);
                AlbumArt = GetArt(file.Tag.Pictures);
            }
        }

        private BitmapImage GetArt(TagLib.IPicture[] pictures)
        {
            BitmapImage image = null;

            if (pictures.Length > 0)
            {
                TagLib.IPicture pic = pictures[0];
                using (MemoryStream ms = new MemoryStream(pic.Data.Data))
                {
                    if (ms != null && ms.Length > 4096)
                    {
                        image = new BitmapImage();
                        image.BeginInit();
                        image.StreamSource = ms;
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.EndInit();
                        image.Freeze();
                    }
                }
            }

            return image;
        }

        public SolidColorBrush ControlBackgroundColor
        {
            get { return UserControl1.ColorLoader.ColorView.LightButtonSolidColorBrush; }
            set
            {
                UserControl1.ColorLoader.ColorView.LightButtonSolidColorBrush = value;
                NotifyPropertyChanged("ControlBackgroundColor");
            }
        }

    }
}
