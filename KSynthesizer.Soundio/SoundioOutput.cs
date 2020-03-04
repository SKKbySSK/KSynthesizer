using System;

namespace KSynthesizer.Soundio
{
    public class SoundioOutput : IAudioOutput
    {
        public event EventHandler<FillBufferEventArgs> FillBuffer;
        
        public AudioFormat Format { get; }
        
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}