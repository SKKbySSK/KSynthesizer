using System;
using System.Collections;

namespace KSynthesizer.Linux
{
    public interface ISynthesizerInput<T>
    {
        event EventHandler<InputEventArgs<T>> Attack;
        event EventHandler<InputEventArgs<T>> Release;
    }

    public class InputEventArgs<T> : EventArgs
    {
        public InputEventArgs(T input)
        {
            Input = input;
        }
        
        public T Input { get; }
    }

    public class InputException : Exception
    {
        public InputException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}