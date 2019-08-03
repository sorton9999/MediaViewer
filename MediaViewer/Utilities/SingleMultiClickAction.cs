using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace MediaViewer.Utilities
{
    public class SingleMultiClickAction
    {
        DispatcherTimer clickTimer;
        Action _singleClickAction = null;
        Action _doubleClickAction = null;
        Dispatcher _dispatcher = null;

        public SingleMultiClickAction(Action singleClickAction, Action multiClickAction, Dispatcher dispatcher)
        {
            _singleClickAction = singleClickAction;
            _doubleClickAction = multiClickAction;
            _dispatcher = dispatcher;
            clickTimer = new DispatcherTimer(new TimeSpan(0, 0, 0, 0, System.Windows.Forms.SystemInformation.DoubleClickTime), DispatcherPriority.Background, new EventHandler(MultiClickEventHandler), _dispatcher);
        }

        public void Element_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (clickTimer.IsEnabled)
            {
                if (e.ClickCount > 1)
                {
                    if (_dispatcher.CheckAccess())
                    {
                        _dispatcher.Invoke(_doubleClickAction);
                    }
                    else
                    {
                        _doubleClickAction.Invoke();
                    }
                    clickTimer.Stop();
                }
            }
            else
            {
                clickTimer.Start();
            }
            e.Handled = true;
        }

        private void MultiClickEventHandler(object sender, EventArgs e)
        {
            if (_dispatcher.CheckAccess())
            {
                _dispatcher.Invoke(_singleClickAction);
            }
            else
            {
                _singleClickAction.Invoke();
            }
            clickTimer.Stop();
        }
    }
}
