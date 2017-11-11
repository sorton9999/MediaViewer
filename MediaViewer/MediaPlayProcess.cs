using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace MediaViewer
{
    public static class MediaPlayProcess
    {
        public static List<string> playList = new List<string>();
        public static Action playAction = new Action(PlayMedia);
        private static Process playProcess = new Process();
        private static bool isInitialized = false;
        private static String nowPlaying = String.Empty;
        public delegate void AddToPlayListDelegate(string song, string length, bool playNow);
        public static event AddToPlayListDelegate AddPlayListEvent;

        public static void Initialize()
        {
            playProcess.StartInfo.FileName = @"C:\Program Files\VideoLAN\VLC\vlc.exe";
            playProcess.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
            isInitialized = true;
        }

        public static List<string> PlayList
        {
            get;
        }

        public static void AddFileToPlay(string file, string length, bool playNow)
        {
            if (playList != null)
            {
                playList.Add(file);
                // Invoke the Add PlayList event with the song title and length of song time
                AddPlayListEvent?.Invoke(file, length, playNow);
            }
        }

        public static String PlayNext()
        {
            String ret = playList.FirstOrDefault();
            playList.Remove(ret);
            return ret;
        }

        private static String PlayArguments()
        {
            String retStr = String.Empty;

            foreach (var item in playList)
            {
                retStr = String.Concat(retStr, (item + " "));
                playList.Remove(item);
            }
            return retStr;
        }

        private static void PlayMedia()
        {
            if (isInitialized)
            {
                playProcess.StartInfo.Arguments = PlayArguments();
                playProcess.Start();
                playProcess.WaitForExit();
            }
        }

    }
}
