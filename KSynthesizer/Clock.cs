using System;
using System.Threading;

namespace KSynthesizer
{
    public class Clock
    {
        private Thread thread;
        private readonly object lockObj = new object();

        public event EventHandler Tick;
        
        public Clock(TimeSpan interval)
        {
            Interval = interval;
        }

        public void Start()
        {
            thread = new Thread(Runner);
            IsRunning = true;
            thread.Start();
        }

        public void Stop()
        {
            IsRunning = false;
            lock (lockObj)
            {
            }
        }

        private void Runner()
        {
            lock (lockObj)
            {
                while (IsRunning)
                {
                    Tick?.Invoke(this, EventArgs.Empty);
                    Thread.Sleep(Interval);
                }
            }
        }

        public TimeSpan Interval { get; set; }

        public bool IsRunning { get; private set; } = false;
    }
}