using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MediaViewer.Controls
{
    /// <summary>
    /// Interaction logic for PlayerControl.xaml
    /// </summary>
    public partial class PlayerControl : UserControl
    {
        public delegate void PlayModeDelegate(PlayerControl.PlayerModeEnum playMode, MouseButtonEventArgs args);
        public static event PlayModeDelegate PlayEvent;

        public enum PlayerModeEnum { PLAYER_MODE_STOP, PLAYER_MODE_PLAY, PLAYER_MODE_PAUSE, PLAYER_MODE_FF, PLAYER_MODE_RW };

        private static PlayerControl _instance;
        PlayerModeEnum _playerMode = PlayerModeEnum.PLAYER_MODE_STOP;

        public PlayerControl()
        {
            InitializeComponent();
            _instance = this;
        }

        public PlayerModeEnum PlayerMode
        {
            get { return _playerMode; }
            private set { _playerMode = value; }
        }

        public void SetPlay()
        {
            PlayPauseVisible(PlayerModeEnum.PLAYER_MODE_PLAY);
        }

        public void Reset()
        {
            _playerMode = PlayerModeEnum.PLAYER_MODE_STOP;
            PlayPauseVisible(_playerMode);
        }

        public static PlayerControl PlayControl()
        {
            return _instance;
        }

        private void RewindBtn_Action(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Rewind");
            PlayerMode = PlayerModeEnum.PLAYER_MODE_RW;
            PlayEvent?.Invoke(PlayerModeEnum.PLAYER_MODE_RW, e);
        }

        private void FastFwdBtn_Action(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Fast Forward");
            PlayerMode = PlayerModeEnum.PLAYER_MODE_FF;
            PlayEvent?.Invoke(PlayerModeEnum.PLAYER_MODE_FF, e);
        }

        private void PlayBtn_Action(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Play");
            PlayPauseVisible(PlayerModeEnum.PLAYER_MODE_PLAY);
            PlayEvent?.Invoke(PlayerModeEnum.PLAYER_MODE_PLAY, e);
        }

        private void PauseBtn_Action(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Pause");
            PlayPauseVisible(PlayerModeEnum.PLAYER_MODE_PAUSE);
            PlayEvent?.Invoke(PlayerModeEnum.PLAYER_MODE_PAUSE, e);
        }

        private void PlayPauseVisible(PlayerModeEnum mode)
        {
            PlayerMode = mode;
            PauseBtn.Visibility = ((PlayerMode == PlayerModeEnum.PLAYER_MODE_PLAY) ? Visibility.Visible : Visibility.Hidden);
            PlayBtn.Visibility = (((PlayerMode == PlayerModeEnum.PLAYER_MODE_PAUSE)
                                   || (PlayerMode == PlayerModeEnum.PLAYER_MODE_STOP)) ? Visibility.Visible : Visibility.Hidden);
        }

    }
}
