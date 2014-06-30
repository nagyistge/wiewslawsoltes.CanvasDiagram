using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;

namespace CanvasDiagram.Droid
{
    public class BackgroundService<T>
    {
        private DataHolder<T> holder = null;
        private Thread thread = null;

        public void Start(Action<T> action, T data)
        {
            if (thread != null)
            {
                return;
            }

            holder = new DataHolder<T>(action, data, true);
            thread = new Thread(new ThreadStart(holder.Loop));
            thread.Start();
        }

        public void Stop()
        {
            if (thread == null)
            {
                return;
            }

            holder.SetRunning(false);
            lock (holder.Sync)
            {
                Monitor.Pulse(holder.Sync);
            }

            thread.Join();
            thread = null;
            holder = null;
        }
            
        public bool HandleEvent(T data, Action<T, T> copy, int timeout)
        {
            return holder != null ? 
                holder.SetData(data, copy, timeout) : 
                false;
        }
    }
}
