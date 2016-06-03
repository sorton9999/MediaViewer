using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;


namespace MediaViewer
{
    public class MediaItem
    {
        public int Id { get; set; }
        public string Item { get; set; }
        public MediaItem(int id, string item)
        {
            Id = id;
            Item = item;
        }
    }

    public class MediaPlayProducerConsumer : ProducerConsumerWorker
    {
        private static MediaPlayProducerConsumer _instance;

        public static MediaPlayProducerConsumer Instance()
        {
            return _instance ?? ( _instance = new MediaPlayProducerConsumer() );
        }

        protected MediaPlayProducerConsumer()
        {
            WorkLength = 0;
            workEvent += MediaPlayProducerConsumer_workEvent;
            EnqueueOnce = MainWindow.IsProcessRunning("vlc");
        }

        void MediaPlayProducerConsumer_workEvent(object workItem)
        {
            string file = workItem as string;
            if (file != null)
            {
                GetPlayProcess(file).Start();
            }
        }

        private Process GetPlayProcess(string file)
        {
            Process process = new Process();
            process.StartInfo.FileName = @"C:\Program Files\VideoLAN\VLC\vlc.exe";
            process.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
            string args = String.Empty;
            if (EnqueueOnce)
            {
                args = "--playlist-enqueue ";
                //args = "--playlist-autostart ";
            }
            args = String.Concat(args, EncodeFile(file, EnqueueOnce));
            process.StartInfo.Arguments = args;
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.WorkingDirectory = Path.GetDirectoryName(file);
            return process;
        }

        private String EncodeFile(String filePath, bool completePath = false)
        {
            String retStr = String.Empty;
            if (completePath)
            {
                retStr = "\"" + filePath + "\"";
            }
            else
            {
                retStr = "\"" + Path.GetFileName(filePath) + "\"";
            }
            return retStr;
        }

        public static void PlayFile(List<string> filesToPlay)
        {
            foreach (var file in filesToPlay)
            {
                int id = MediaPlayProducerConsumer.Instance().WorkLength + 1;
                MediaPlayProducerConsumer.Instance().Produce
                    (
                        new WorkItem
                            (
                                id, file
                            )
                    );
            }
            MediaPlayProducerConsumer.Instance().Wait();
        }
    }
}
