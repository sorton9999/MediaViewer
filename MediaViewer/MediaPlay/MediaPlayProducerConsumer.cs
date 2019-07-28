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
    /// <summary>
    /// A threading class that follows the Producer/Consumer pattern.  The producer
    /// creates threads and pass items to the consumer handler to be acted upon
    /// when each thread's handler is called.  This class also follows the
    /// Singleton pattern.
    /// </summary>
    public class MediaPlayProducerConsumer : ProducerConsumerWorker
    {
        /// <summary>
        /// A static reference to the object of this class
        /// </summary>
        private static MediaPlayProducerConsumer _instance;

        /// <summary>
        /// The public accessor to the static instance
        /// </summary>
        /// <returns>The static reference</returns>
        public static MediaPlayProducerConsumer Instance()
        {
            return _instance ?? ( _instance = new MediaPlayProducerConsumer() );
        }

        /// <summary>
        /// Constructor -- A work handler is set up as well as a state based
        /// on whether or not the media player is already running.
        /// </summary>
        protected MediaPlayProducerConsumer()
        {
            WorkLength = 0;
            workEvent += MediaPlayProducerConsumer_workEvent;
            EnqueueOnce = MainWindow.IsProcessRunning("vlc");
        }

        /// <summary>
        /// The work handler each thread calls to do work.  The work is to
        /// play the media file passed in as the work item.
        /// </summary>
        /// <param name="workItem">The passed item to do work on</param>
        void MediaPlayProducerConsumer_workEvent(object workItem)
        {
            // Grab the media file
            string file = workItem as string;
            if (file != null)
            {
                // Start playing the media file
                GetPlayProcess(file).Start();
            }
        }

        /// <summary>
        /// Set the process info to allow the media player to start as a
        /// new process which will be started in the work handler.
        /// </summary>
        /// <param name="file">The media file to play</param>
        /// <returns>The VLC process info to start</returns>
        private Process GetPlayProcess(string file)
        {
            Process process = new Process();
            // The location of the player is set in the Config View window
            process.StartInfo.FileName = ConfigView.ConfigViewModel.VlcPath;
            //process.StartInfo.FileName = @"C:\Program Files\VideoLAN\VLC\vlc.exe";
            process.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
            string args = String.Empty;
            // Based on the value of this flag, we either start the player with the file
            // to play (False) or add it to an existing media player instance (True).
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

        /// <summary>
        /// Encode the given file to make it useable by the media player
        /// </summary>
        /// <param name="filePath">The path to the file to play</param>
        /// <param name="completePath">Is this a complete path?</param>
        /// <returns>The encoded string of the file path</returns>
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

        /// <summary>
        /// A static method used to play the given list of files.
        /// </summary>
        /// <param name="filesToPlay">The list of files to play</param>
        public static async Task PlayFile(List<string> filesToPlay)
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
