using System;
using System.Threading.Tasks;

namespace TestTool.Platform
{
    public class CommonInput : IConsoleInput
    {
        public event EventHandler<InputEventArgs> Attack;
        public event EventHandler Release;
        public event EventHandler Exit;
        private bool stop = false;

        public void Listen()
        {
            stop = false;
            Task.Run(() =>
            {
                while (!stop)
                {
                    var c = (char) Console.Read();
                    if (c == 'q')
                    {
                        Exit?.Invoke(this, EventArgs.Empty);
                        break;
                    }
                    else if (c == 'a')
                    {
                        Attack?.Invoke(this, new InputEventArgs(){ Frequency = 500 });
                    }
                    else if (c == 'r')
                    {
                        Release?.Invoke(this, EventArgs.Empty);
                    }
                }
            });
        }

        public void Stop()
        {
            stop = true;
        }
    }
}