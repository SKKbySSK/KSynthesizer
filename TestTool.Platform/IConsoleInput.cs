using System;

namespace TestTool.Platform
{
    public class InputEventArgs : EventArgs
    {
        public float Frequency { get; set; }
    }
    
    public interface IConsoleInput
    {
        event EventHandler<InputEventArgs> Attack;
        event EventHandler Release;
        event EventHandler Exit;
        
        void Listen();
        void Stop();
    }
}