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
        /// The EnqueueOnce flag to indicate which argument to give the player
        /// when starting the player for the first time vs. calling again when the
        /// player is already started and you want to just add the song to the
        /// play list.
        /// </summary>
        private bool EnqueueOnce = false;

        /// <summary>
        /// The location in the list where the title is playing
        /// </summary>
        private int playIdx = -1;

        /// <summary>
        /// The location in the list where the current title is playing
        /// </summary>
        private int titlePlayIdx = -1;

        /// <summary>
        /// Is a title currently playing
        /// </summary>
        private bool playing = false;

        /// <summary>
        /// Are we adding a "Play Next" title
        /// </summary>
        private bool playNext = false;

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
        /// The public accessor for the play process object
        /// </summary>
        public MediaPlayProcess PlayProcess
        {
            get { return MainWindow.PlayProcess; }
        }

        /// <summary>
        /// Public accessor for total viewmodel objects in the PlayList
        /// </summary>
        public int TitleCount
        {
            get { return MainWindow.PlayListItems.Count; }
        }

        /// <summary>
        /// Public accessor for location in the list where the title is playing
        /// </summary>
        public int PlayIndex
        {
            get { return playIdx; }
        }

        /// <summary>
        /// Public accessor for location in the list where the current title is playing
        /// </summary>
        public int TitlePlayIndex
        {
            get { return titlePlayIdx; }
        }

        /// <summary>
        /// Public accessor for Is a title currently playing
        /// </summary>
        public bool TitlePlaying
        {
            get { return playing; }
        }

        /// <summary>
        /// Public accessor for Are we adding a "Play Next" title
        /// </summary>
        public bool TitlePlayingNext
        {
            get { return playNext; }
        }

        /// <summary>
        /// Public accessor for Are we inserting a title
        /// </summary>
        public bool InsertingTitle
        {
            get { return (PlayIndex > -1); }
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
        /// Sets the play list insert indices.  This is called when the menu items
        /// that play titles immediately or insert titles directly after the
        /// playing title are chosen.
        /// </summary>
        /// <param name="playType">Which menu item is chosen</param>
        public void SetPlayerIndices(string playType)
        {
            titlePlayIdx = -1;
            playIdx = -1;
            playing = false;
            foreach (PlayListViewModel p in MainWindow.PlayListItems)
            {
                ++titlePlayIdx;
                if (p.NowPlaying)
                {
                    playing = true;
                    break;
                }
            }
            // The "Play Song" menu item
            if (playType == "0")
            {
                playIdx = TitleCount;
            }
            // The "Play Next" menu item
            else if (playType == "1")
            {
                playNext = true;
                if (playing)
                {
                    playIdx = titlePlayIdx + 1;
                }
                else
                {
                    playIdx = TitleCount;
                }
            }
        }

        /// <summary>
        /// Insert the given PlayList viewmodel object into the PlayList
        /// </summary>
        /// <param name="idx">Where to insert</param>
        /// <param name="plm">The viewmodel object</param>
        public void InsertTitleToPlayList(int idx, PlayListViewModel plm)
        {
            try
            {
                if (idx == TitleCount + 1)
                {
                    AddTitleToPlayList(plm);
                }
                else
                {
                    MainWindow.PlayListItems.Insert(idx, plm);
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Adds the given viewmodel object to the end of the PlayList
        /// </summary>
        /// <param name="plm">The viewmodel object</param>
        public void AddTitleToPlayList(PlayListViewModel plm)
        {
            MainWindow.PlayListItems.Add(plm);
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
            if (MainWindow.PlayProcess != null)
            {
                MainWindow.PlayProcess.AddTrack(title);
            }
        }

        private void AddTitle(string title, int idx)
        {
            if (MainWindow.PlayProcess != null)
            {
                MainWindow.PlayProcess.AddTrack(title, idx);
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
        /// <param name="playIdx">The index of the location where the title to play is in the list</param>
        /// <param name="playNext">Are we playing the title after the current playing title</param>
        public static async Task PlayFileAsync(List<string> filesToPlay, int playIdx, bool playNext)
        {
            int addIdx = playIdx;
            List<Task<string>> tasks = new List<Task<string>>();

            foreach (var file in filesToPlay)
            {
                tasks.Add(Task.Run(() => GetPlayTitle(file)));
            }

            var results = await Task.WhenAll(tasks);

            foreach (var item in results)
            {
                if (playIdx > -1)
                {
                    Instance().AddTitle(item, addIdx++);
                }
                else
                {
                    Instance().AddTitle(item);
                }
            }
            if ((playIdx > -1) && !playNext)
            {
                MainWindow.PlayProcess.Play(true, playIdx);
            }
        }

    }
}
