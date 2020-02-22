namespace KSynthesizer.Sources
{
    public abstract class FunctionalAudioSourceBase : IAudioSource
    {
        public FunctionalAudioSourceBase(int bufferSize)
        {
            BufferSize = bufferSize;
        }
        
        public int BufferSize { get; }
        
        public AudioFormat Format { get; protected set; }

        public double Position { get; protected set; }

        protected float[] GetNextBuffer()
        {
            return new float[BufferSize];
        }

        public abstract float[] Next();
    }
}