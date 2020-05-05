using System;

namespace KSynthesizer
{
    public interface IClock
    {
        event EventHandler Tick;
        bool IsRunning { get; }
    }
}