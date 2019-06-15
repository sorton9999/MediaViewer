using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using LibVLCSharp.Shared;

namespace MediaViewer
{
    public class MediaPlayProcess
    {
        public enum MediaPlayStateEnum {
            MEDIA_UNINIT = -99, MEDIA_DONTCARE = -2, MEDIA_ERROR = -1,
            MEDIA_BUFFERING, MEDIA_OPENING, MEDIA_ENDED, MEDIA_STOP,
            MEDIA_PLAY, MEDIA_PAUSE, MEDIA_FASTFWD, MEDIA_REWIND
        };

        public delegate void MediaAction(object sender, EventArgs e);
        public event MediaAction PlayEvent;
        public event MediaAction StopEvent;
        public event MediaAction RewindEvent;
        public event MediaAction FastForwardEvent;
        public event MediaAction PauseEvent;

        //private List<string> playList = new List<string>();
        public Action playAction;
        private Process playProcess = new Process();
        private bool isInitialized = false;
        int trackIdx = 0;
        MediaPlayStateEnum _state = MediaPlayStateEnum.MEDIA_UNINIT;

        readonly LibVLC _libVLC;
        readonly MediaPlayer _mediaPlayer;
        private MediaList _mediaList;
        const long OFFSET = 5000;


        public MediaPlayProcess()
        {
            Core.Initialize();

            _libVLC = new LibVLC();
            _mediaPlayer = new MediaPlayer(_libVLC);
            _mediaList = new MediaList(_libVLC);

            Initialize();
        }

        public void Dispose()
        {
            this._mediaList.Dispose();
            this._mediaPlayer.Dispose();
            this._libVLC.Dispose();
        }

        public void Initialize()
        {
            //playProcess.StartInfo.FileName = @"C:\Program Files\VideoLAN\VLC\vlc.exe";
            //playProcess.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;

            //_mediaPlayer.Media = new Media(_libVLC, "C:\\Users\\Orton\\Music\\05 Fly By Night.wav", Media.FromType.FromPath);
            AddTrack("F:\\Music Archive\\The Beatles\\Let it be (2009) Digital Remaster\\05 Dig it.flac");
            AddTrack("F:\\Music Archive\\The Beatles\\Let it be (2009) Digital Remaster\\07 Maggie Mae.flac");

            _mediaList.Lock();
            _mediaPlayer.Media = _mediaList[trackIdx];
            _mediaPlayer.Media.AddOption(":no-video");
            _mediaList.Unlock();

            _mediaPlayer.TimeChanged += _mediaPlayer_TimeChanged;
            _mediaPlayer.PositionChanged += _mediaPlayer_PositionChanged;
            _mediaPlayer.LengthChanged += _mediaPlayer_LengthChanged;
            _mediaPlayer.EndReached += _mediaPlayer_EndReached;
            _mediaPlayer.Playing += _mediaPlayer_Playing;
            _mediaPlayer.Paused += _mediaPlayer_Paused;
            _mediaPlayer.Forward += _mediaPlayer_Forward;
            _mediaPlayer.Backward += _mediaPlayer_Backward;
            _mediaPlayer.MediaChanged += _mediaPlayer_MediaChanged;
            _mediaPlayer.TitleChanged += _mediaPlayer_TitleChanged;
            _mediaPlayer.Stopped += _mediaPlayer_Stopped;
            _mediaPlayer.SetVolumeCallback(new MediaPlayer.LibVLCVolumeCb(MediaPlayer_ChangeVolume));
            _mediaPlayer.Volume = 25;

            RewindEvent = new MediaAction(_mediaPlayer_Backward);
            FastForwardEvent = new MediaAction(_mediaPlayer_Forward);

            isInitialized = true;
        }

        private void _mediaPlayer_Stopped(object sender, EventArgs e)
        {
            Debug.WriteLine("Title Stopped");
        }

        private void _mediaPlayer_TitleChanged(object sender, MediaPlayerTitleChangedEventArgs e)
        {
            Debug.WriteLine("Title Changed");
        }

        public int TrackCount()
        {
            return _mediaPlayer.AudioTrackCount;
        }

        public string TrackName(int idx)
        {
            return _mediaPlayer.AudioTrackDescription[idx].Name;
        }

        public bool AddTrack(string mediaPath)
        {
            bool retVal = false;
            _mediaList.Lock();
            retVal = _mediaList.AddMedia(new Media(_libVLC, mediaPath, Media.FromType.FromPath));
            _mediaList.Unlock();
            return retVal;
        }

        public bool RemoveTrack(string mediaPath)
        {
            bool retVal = false;
            _mediaList.Lock();
            int idx = _mediaList.IndexOf(new Media(_libVLC, mediaPath, Media.FromType.FromPath));
            if (idx > -1)
            {
                retVal = _mediaList.RemoveIndex(idx);
            }
            _mediaList.Unlock();
            return retVal;
        }

        public void SetRate(float rate)
        {
            if (rate > 1F)
            {
                _state = MediaPlayStateEnum.MEDIA_FASTFWD;
            }
            _mediaPlayer.SetRate(rate);
        }

        public MediaPlayStateEnum GetState()
        {
            MediaPlayStateEnum state = MediaPlayStateEnum.MEDIA_UNINIT;
            switch (_mediaPlayer.State)
            {
                case VLCState.Buffering:
                    state = MediaPlayStateEnum.MEDIA_BUFFERING;
                    break;
                case VLCState.Ended:
                    state = MediaPlayStateEnum.MEDIA_ENDED;
                    break;
                case VLCState.Error:
                    state = MediaPlayStateEnum.MEDIA_ERROR;
                    break;
                case VLCState.NothingSpecial:
                    state = MediaPlayStateEnum.MEDIA_DONTCARE;
                    break;
                case VLCState.Opening:
                    state = MediaPlayStateEnum.MEDIA_OPENING;
                    break;
                case VLCState.Paused:
                    state = MediaPlayStateEnum.MEDIA_PAUSE;
                    break;
                case VLCState.Playing:
                    state = MediaPlayStateEnum.MEDIA_PLAY;
                    break;
                case VLCState.Stopped:
                    state = MediaPlayStateEnum.MEDIA_STOP;
                    break;
                default:
                    state = MediaPlayStateEnum.MEDIA_DONTCARE;
                    break;
            }
            return state;
        }

        private void MediaPlayerPlay()
        {
            _mediaList.Lock();
            _mediaPlayer.Play(_mediaList[trackIdx]);
            _mediaList.Unlock();
        }

        private void InvokePlayer()
        {
            Action playAction = new Action(MediaPlayerPlay);
            playAction.BeginInvoke(new AsyncCallback(result =>
            {
                (result.AsyncState as Action).EndInvoke(result);

            }), playAction);
        }

        public void Play(bool play)
        {
            if (play)
            {
                if (IsFastForward())
                {
                    _mediaPlayer.SetRate(1F);
                }
                else
                {
                    //_mediaPlayer.Play();
                    InvokePlayer();
                }
            }
            else
            {
                _mediaPlayer.Pause();
                _mediaPlayer.SetRate(1F);
            }
        }

        /*
        public void Play(string mediaPath)
        {
            if (String.IsNullOrEmpty(mediaPath))
            {
                return;
            }
            if (_mediaPlayer.IsPlaying)
            {
                _mediaPlayer.Stop();
            }
            Action secondFooAsync = new Action(MyPlay);

            secondFooAsync.BeginInvoke(new AsyncCallback(result =>
            {
                (result.AsyncState as Action).EndInvoke(result);

            }), secondFooAsync);
            //_mediaPlayer.Play(new Media(_libVLC, mediaPath, Media.FromType.FromPath));
            //Debug.WriteLine(GetState().ToString());
        }
        */

        protected void MyPlay()
        {
            _mediaList.Lock();
            _mediaPlayer.Play(_mediaList[trackIdx]);
            _mediaList.Unlock();
        }

        public void Stop()
        {
            if (_mediaPlayer.IsPlaying)
            {
                _mediaPlayer.Stop();
            }
        }

        public bool IsFastForward()
        {
            return GetState() == MediaPlayStateEnum.MEDIA_FASTFWD;
        }

        public bool IsRewind()
        {
            return GetState() == MediaPlayStateEnum.MEDIA_REWIND;
        }

        private void MediaPlayer_ChangeVolume(IntPtr data, float volume, bool mute)
        {
            if (mute)
            {
                _mediaPlayer.Volume = 0;
            }
            else
            {
                _mediaPlayer.Volume = (int)(100F * volume);
            }
        }

        private void _mediaPlayer_Backward(object sender, EventArgs e)
        {
            Debug.WriteLine("Rewind");
            _state = MediaPlayStateEnum.MEDIA_REWIND;
            _mediaPlayer.Time -= OFFSET;
        }

        private void _mediaPlayer_Forward(object sender, EventArgs e)
        {
            Debug.WriteLine("Forward");
            _state = MediaPlayStateEnum.MEDIA_FASTFWD;
            _mediaPlayer.Time += OFFSET;
        }

        private void _mediaPlayer_Paused(object sender, EventArgs e)
        {
            Debug.WriteLine("Paused");
        }

        private void _mediaPlayer_Playing(object sender, EventArgs e)
        {
            Debug.WriteLine("Playing");
        }

        private void _mediaPlayer_EndReached(object sender, EventArgs e)
        {
            Debug.WriteLine("End Reached");
            Debug.WriteLine(GetState().ToString());
            int mediaCount = -1;
            _mediaList.Lock();
            mediaCount = _mediaList.Count;
            ++trackIdx;
            _mediaList.Unlock();
            if (mediaCount > trackIdx)
            {
                //Play(media.Mrl);
                Play(true);
            }
        }

        private void _mediaPlayer_LengthChanged(object sender, MediaPlayerLengthChangedEventArgs e)
        {
            Debug.WriteLine("Length Changed");
        }

        private void _mediaPlayer_PositionChanged(object sender, MediaPlayerPositionChangedEventArgs e)
        {
            Debug.WriteLine("Position Changed {0}", e.Position);
        }

        private void _mediaPlayer_TimeChanged(object sender, MediaPlayerTimeChangedEventArgs e)
        {
            Debug.WriteLine("Time Changed: {0}", e.Time);
        }

        private void _mediaPlayer_MediaChanged(object sender, MediaPlayerMediaChangedEventArgs e)
        {
            Debug.WriteLine("Media Changed");
        }

        //public void AddFileToPlay(string file)
        //{
        //    if (playList != null)
        //    {
        //        playList.Add(file);
        //    }
        //}

        //private String PlayArguments()
        //{
        //    String retStr = String.Empty;

        //    foreach (var item in playList)
        //    {
        //        retStr = String.Concat(retStr, (item + " "));
        //        playList.Remove(item);
        //    }
        //    return retStr;
        //}

        //private void PlayMedia()
        //{
        //    if (isInitialized)
        //    {
        //        playProcess.StartInfo.Arguments = PlayArguments();
        //        playProcess.Start();
        //        playProcess.WaitForExit();
        //    }
        //}

    }
}
