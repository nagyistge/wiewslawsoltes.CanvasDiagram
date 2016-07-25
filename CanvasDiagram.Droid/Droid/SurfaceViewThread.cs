using System;
using System.Threading;

namespace CanvasDiagram.Droid
{
    public class SurfaceViewThread<T>
    {
        private SurfaceViewData<T> viewData = null;
        private Thread viewThread = null;

        public void Start(Action<T> action, T data)
        {
            if (viewThread != null)
            {
                return;
            }

            viewData = new SurfaceViewData<T>(action, data, true);
            viewThread = new Thread(new ThreadStart(viewData.Loop));
            viewThread.Start();
        }

        public void Stop()
        {
            if (viewThread == null)
            {
                return;
            }

            viewData.SetRunning(false);
            lock (viewData.Sync)
            {
                Monitor.Pulse(viewData.Sync);
            }

            viewThread.Join();
            viewThread = null;
            viewData = null;
        }

        public bool HandleEvent(T data, Action<T, T> copy, int timeout)
        {
            return viewData != null ? viewData.SetData(data, copy, timeout) : false;
        }
    }
}
