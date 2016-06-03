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
        private static List<string> playList = new List<string>();
        public static Action playAction = new Action(PlayMedia);
        private static Process playProcess = new Process();
        private static bool isInitialized = false;

        public static void Initialize()
        {
            playProcess.StartInfo.FileName = @"C:\Program Files\VideoLAN\VLC\vlc.exe";
            playProcess.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
            isInitialized = true;
        }

        public static void AddFileToPlay(string file)
        {
            if (playList != null)
            {
                playList.Add(file);
            }
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
