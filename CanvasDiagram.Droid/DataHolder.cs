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
    public class DataHolder<T>
    {
        public readonly object Sync = new object();

        public bool IsRunning { get; private set; }

        public T Data { get; private set; }

        public Action<T> Action { get; private set; }

        public DataHolder(Action<T> action, T data, bool isRunning)
        {
            Data = data;
            Action = action;
            IsRunning = isRunning;
        }

        public void SetAction(Action<T> action)
        {
            lock (Sync)
                Action = action;
        }

        public void SetRunning(bool isRunning)
        {
            IsRunning = isRunning;
        }

        public bool SetData(T data, Action<T, T> copy, int timeout)
        {
            if (Monitor.TryEnter(Sync, timeout) == false)
                return false;

            copy(data, Data);

            Monitor.Pulse(Sync);
            Monitor.Exit(Sync);

            return true;
        }

        public void Loop()
        {
            while (IsRunning)
            {
                lock (Sync)
                {
                    if (Action != null)
                        Action(Data);

                    Monitor.Wait(Sync);
                }
            }
        }
    }
}
