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

        public delegate bool Adder(string file);
        public delegate bool Remover(int idx);
        public delegate void AdvancePlayLabel(string e);
        public delegate void MediaAction(object sender, EventArgs e);
        public event MediaAction PlayEvent;
        public event MediaAction StopEvent;
        public event MediaAction RewindEvent;
        public event MediaAction FastForwardEvent;
        public event MediaAction PauseEvent;

        public Action playAction;
        private Process playProcess = new Process();
        private bool isInitialized = false;
        int trackIdx = 0;
        MediaPlayStateEnum _state = MediaPlayStateEnum.MEDIA_UNINIT;

        readonly LibVLC _libVLC;
        readonly MediaPlayer _mediaPlayer;
        MediaList _mediaList;
        const long OFFSET = 5000;

        private MainWindow _parent = null;
        private List<string> titlesToPlay = new List<string>();

        public MediaPlayProcess(MainWindow parent)
        {
            Core.Initialize();
            _parent = parent;

            _libVLC = new LibVLC(new string[] { "--one-instance-when-started-from-file" });
            _mediaPlayer = new MediaPlayer(_libVLC);
            _mediaList = new MediaList(_libVLC);

            _libVLC.Log += (sender, e) => System.Diagnostics.Debug.WriteLine($"[{e.Level}] {e.Module}:{e.Message}");

            Initialize();
        }

        public MediaPlayProcess(MainWindow parent, List<string> items)
        {
            Core.Initialize();

            _parent = parent;

            _libVLC = new LibVLC();
            _mediaPlayer = new MediaPlayer(_libVLC);
            _mediaList = new MediaList(_libVLC);

            titlesToPlay = items;

            Initialize();
        }

        public void Dispose()
        {
            this._mediaList.Dispose();
            this._mediaPlayer.Dispose();
            this._libVLC.CloseLogFile();
            this._libVLC.Dispose();
        }

        public void Initialize()
        {
            //playProcess.StartInfo.FileName = @"C:\Program Files\VideoLAN\VLC\vlc.exe";
            //playProcess.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;

            //_mediaPlayer.Media = new Media(_libVLC, "C:\\Users\\Orton\\Music\\05 Fly By Night.wav", Media.FromType.FromPath);
            //AddTrack("F:\\Music Archive\\Genesis\\Duke\\05 Misunderstanding.flac");
            //AddTrack("F:\\Music Archive\\Genesis\\Abacab\\07 Man on the corner.flac");


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
            //_mediaPlayer.Volume = 15;

            RewindEvent = new MediaAction(_mediaPlayer_Backward);
            FastForwardEvent = new MediaAction(_mediaPlayer_Forward);

            isInitialized = true;
        }

        private void _mediaPlayer_Stopped(object sender, EventArgs e)
        {
            Debug.WriteLine("Title Stopped");
            Action reset = () =>
            {
                _parent.workProgressBar.Value = 0;
            };
            _parent.CheckAndInvoke(reset);
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
            retVal = _mediaList.AddMedia(new Media(_libVLC, mediaPath));
            if (retVal && GetState() != MediaPlayStateEnum.MEDIA_PLAY)
            {
                //_mediaPlayer.Media = _mediaList[(_mediaList.Count - 1)];
                _mediaPlayer.Media = _mediaList[0];
                _mediaPlayer.Media.AddOption(":no-video");
            }
            return retVal;
        }

        public bool RemoveTrack(int idx)
        {
            bool retVal = false;
            try
            {
                if (_mediaList[idx].State != VLCState.Playing)
                {
                    retVal = _mediaList.RemoveIndex(idx);
                }
            }
            catch (Exception)
            {
                retVal = false;
            }
            return retVal;
        }

        public bool RemoveTrack(string mediaPath)
        {
            bool retVal = false;
            int idx = _mediaList.IndexOf(new Media(_libVLC, mediaPath));
            if (idx > -1)
            {
                retVal = _mediaList.RemoveIndex(idx);
            }
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

        private void MediaPlayerAdd(string file)
        {
            try
            {
                AddTrack(file);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
        }

        private void MediaPlayerPlay()
        {
            try
            {
                if ((_mediaList.Count > 0) && (trackIdx >= 0) && (trackIdx < _mediaList.Count))
                {
                    _mediaPlayer.Play(_mediaList[trackIdx]);
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
        }

        public bool InvokeAdder(string file)
        {
            Adder adder = new Adder(AddTrack);
            bool ret = false;
            var res = adder.BeginInvoke(file, new AsyncCallback(result =>
            {
                ret = (result.AsyncState as Adder).EndInvoke(result);
            }), adder);
            return (res.IsCompleted && ret);
        }

        public bool InvokeRemover(int idx)
        {
            Remover rem = new Remover(RemoveTrack);
            bool ret = false;
            var res = rem.BeginInvoke(idx, new AsyncCallback(result =>
            {
                ret = (result.AsyncState as Remover).EndInvoke(result);
            }), rem);
            return (res.IsCompleted && ret);
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
                if ((trackIdx >= 0) && (trackIdx < MainWindow.PlayListItems.Count))
                {
                    MainWindow.SetTrackItems(trackIdx);
                    if (IsFastForward())
                    {
                        _mediaPlayer.SetRate(1F);
                    }
                    else
                    {
                        InvokePlayer();
                    }
                }
            }
            else
            {
                _mediaPlayer.Pause();
                _mediaPlayer.SetRate(1F);
            }
        }

        public void Play(bool play, int idx)
        {
            trackIdx = idx;
            Play(play);
        }

        public void NextTrack()
        {
            ++trackIdx;
            if (trackIdx > _mediaList.Count - 1)
            {
                trackIdx = _mediaList.Count - 1;
                return;
            }
            Play(true);
        }

        public void PreviousTrack()
        {
            --trackIdx;
            if (trackIdx < 0)
            {
                trackIdx = 0;
            }
            Play(true);
        }

        public void Stop()
        {
            if (_mediaPlayer.IsPlaying)
            {
                _mediaPlayer.Stop();
            }
        }

        public void SetPosition(float position)
        {
            _mediaPlayer.Position = position;
        }

        public bool IsFastForward()
        {
            return GetState() == MediaPlayStateEnum.MEDIA_FASTFWD;
        }

        public bool IsRewind()
        {
            return GetState() == MediaPlayStateEnum.MEDIA_REWIND;
        }

        public void MediaPlayer_ChangeVolume(IntPtr data, float volume, bool mute)
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

        public void _mediaPlayer_Backward(object sender, EventArgs e)
        {
            Debug.WriteLine("Rewind");
            _state = MediaPlayStateEnum.MEDIA_REWIND;
            _mediaPlayer.Time -= OFFSET;
        }

        public void _mediaPlayer_Forward(object sender, EventArgs e)
        {
            Debug.WriteLine("Forward");
            _state = MediaPlayStateEnum.MEDIA_FASTFWD;
            SetRate(2.0F);
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
            mediaCount = _mediaList.Count;
            ++trackIdx;
            if (mediaCount > trackIdx)
            {
                Play(true);
            }
            //else
            //{
            //    Play(false);
            //}
        }

        private void _mediaPlayer_LengthChanged(object sender, MediaPlayerLengthChangedEventArgs e)
        {
            Debug.WriteLine("Length Changed");
        }

        private void _mediaPlayer_PositionChanged(object sender, MediaPlayerPositionChangedEventArgs e)
        {
            Debug.WriteLine("Position Changed {0}", e.Position);
            Action adv = () =>
            {
                _parent.workProgressBar.Value = e.Position * 100;
            };
            _parent.CheckAndInvoke(adv);
        }

        private void _mediaPlayer_TimeChanged(object sender, MediaPlayerTimeChangedEventArgs e)
        {
            Debug.WriteLine("Time Changed: {0}", e.Time);

            if ((trackIdx < 0) || (trackIdx >= _mediaList.Count))
            {
                return;
            }

            double playSecs = (e.Time / 1000) % 60;
            double playMins = ((e.Time / 1000) / 60) % 60;
            double playHours = (((e.Time / 1000) / 60) / 60) % 60;
            string str = String.Format("{0}:{1:00}:{2:00}", playHours, playMins, playSecs);
            long totalDuration = _mediaList[trackIdx].Duration;
            double totalSecs = (totalDuration / 1000) % 60;
            double totalMins = ((totalDuration / 1000) / 60) % 60;
            double totalHours = (((totalDuration / 1000) / 60) / 60) % 60;
            string dur = String.Format("{0}:{1:00}:{2:00}", totalHours, totalMins, totalSecs);
            AdvancePlayLabel adv = (s) =>
            {
                _parent.lblTotalTime.Content = dur;
                _parent.lblPlayTime.Content = s;
            };
            if (!_parent.Dispatcher.CheckAccess())
            {
                _parent.Dispatcher.Invoke(adv, str);
            }
            else
            {
                adv.Invoke(str);
            }
        }

        private void _mediaPlayer_MediaChanged(object sender, MediaPlayerMediaChangedEventArgs e)
        {
            Debug.WriteLine("Media Changed");
            Action reset = () =>
            {
                _parent.workProgressBar.Value = 0;
            };
            _parent.CheckAndInvoke(reset);
        }


    }
}
