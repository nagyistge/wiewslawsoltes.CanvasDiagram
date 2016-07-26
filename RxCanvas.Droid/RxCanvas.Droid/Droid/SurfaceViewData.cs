// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Threading;

namespace RxCanvas.Droid
{
    public class SurfaceViewData
    {
        private readonly object _sync = new object();
        private bool _isRunning;
        private Action _action;
        private Thread _thread = null;

        public void Start(Action action)
        {
            if (_thread != null)
            {
                return;
            }

            _action = action;
            _isRunning = true;
            _thread = new Thread(new ThreadStart(Loop));
            _thread.Start();
        }

        public void Stop()
        {
            if (_thread == null)
            {
                return;
            }

            SetRunning(false);
            lock (_sync)
            {
                Monitor.Pulse(_sync);
            }

            _thread.Join();
            _thread = null;
        }

        public void SetAction(Action action)
        {
            lock (_sync)
            {
                _action = action;
            }
        }

        public void SetRunning(bool isRunning)
        {
            _isRunning = isRunning;
        }

        public bool Tick(int timeout)
        {
            if (Monitor.TryEnter(_sync, timeout) == false)
            {
                return false;
            }

            Monitor.Pulse(_sync);
            Monitor.Exit(_sync);
            return true;
        }

        public void Loop()
        {
            while (_isRunning)
            {
                lock (_sync)
                {
                    if (_action != null)
                    {
                        _action();
                    }
                    Monitor.Wait(_sync);
                }
            }
        }
    }
}
