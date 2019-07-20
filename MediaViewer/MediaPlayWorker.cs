using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer
{
    /// <summary>
    /// A class that provides the asynchronous Play method.  This class also follows the
    /// Singleton pattern.
    /// </summary>
    public class MediaPlayWorker
    {
        /// <summary>
        /// The static instance
        /// </summary>
        private static MediaPlayWorker _instance = null;

        /// <summary>
        /// A static play process object that provides media play capabilities.  We're using
        /// this to load the files to play.
        /// </summary>
        private static MediaPlayProcess _playProcess = null;

        /// <summary>
        /// The EnqueueOnce flag to indicate which argument to give the player
        /// when starting the player for the first time vs. calling again when the
        /// player is already started and you want to just add the song to the
        /// play list.
        /// </summary>
        private bool EnqueueOnce = false;

        /// <summary>
        /// Constructor
        /// Set the EnqueueOnce flag depending on if the player is already
        /// started or not.
        /// </summary>
        protected MediaPlayWorker()
        {
            EnqueueOnce = MainWindow.IsProcessRunning("vlc");
        }

        /// <summary>
        /// The static instance caller for the Singleton
        /// </summary>
        /// <returns></returns>
        public static MediaPlayWorker Instance()
        {
            return (_instance ?? (_instance = new MediaPlayWorker()));
        }

        /// <summary>
        /// The static public accessor for the play process object
        /// </summary>
        public static MediaPlayProcess PlayProcess
        {
            get { return _playProcess; }
            private set { _playProcess = value; }
        }

        /// <summary>
        /// Take the given process type and start it.
        /// </summary>
        /// <param name="proc"></param>
        private void StartProcess(Process proc)
        {
            //proc.Start();
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

        private static string GetPlayTitle(string title)
        {
            return title;
        }

        private void AddTitle(string title)
        {
            if (_playProcess != null)
            {
                _playProcess.AddTrack(title);
            }
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
        /// A static asynchronous method used to play the given list of files.
        /// </summary>
        /// <param name="filesToPlay">The list of files to play</param>
        public static async Task PlayFileAsync(List<string> filesToPlay, MediaPlayProcess playProcess)
        {
            _playProcess = playProcess;
            //List<Task<Process>> tasks = new List<Task<Process>>();
            List<Task<string>> tasks = new List<Task<string>>();

            foreach (var file in filesToPlay)
            {
                //tasks.Add(Task.Run(() => Instance().GetPlayProcess(file)));
                tasks.Add(Task.Run(() => GetPlayTitle(file)));
            }

            var results = await Task.WhenAll(tasks);

            foreach (var item in results)
            {
                //Instance().StartProcess(item);
                Instance().AddTitle(item);
            }
        }

    }
}
