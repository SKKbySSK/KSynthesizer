using System;

namespace KSynthesizer
{
    public class FillBufferEventArgs : EventArgs
    {
        public FillBufferEventArgs(AudioFormat format, int minSize, int maxSize)
        {
            Format = format;
            MinimumSize = minSize;
            MaximumSize = maxSize;
        }
        
        public AudioFormat Format { get; }
        
        public int MinimumSize { get; }
        
        public int MaximumSize { get; }
        
        internal float[] Buffer { get; set; }

        public void Configure(float[] buffer)
        {
            Buffer = buffer;
        }
    }
    
    public interface IAudioOutput : IDisposable
    {
        event EventHandler<FillBufferEventArgs> FillBuffer;
        
        AudioFormat Format { get; }
    }
}
