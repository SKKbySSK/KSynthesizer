using System;

namespace KSynthesizer
{
    public class FillBufferEventArgs : EventArgs
    {
        public FillBufferEventArgs(AudioFormat format, int size)
        {
            Format = format;
            Size = size;
        }
        
        public AudioFormat Format { get; }
        
        public int Size { get; }
        
        public float[] Buffer { get; private set; }

        public void Configure(float[] buffer)
        {
            if (buffer.Length != Size)
            {
                throw new Exception("Buffer Size must be equal to Size Property");
            }
            
            Buffer = buffer;
        }
    }

    public class OutputInitializationException : Exception
    {
        public OutputInitializationException(string message) : base(message)
        {
        }
        
        public OutputInitializationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
    
    public interface IAudioOutput : IDisposable
    {
        AudioFormat Format { get; }
        
        TimeSpan DesiredLatency { get; set; }
        
        TimeSpan ActualLatency { get; }

        void Play();

        void Stop();
    }
}
