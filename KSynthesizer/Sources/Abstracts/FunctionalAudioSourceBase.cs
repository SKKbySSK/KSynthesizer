namespace KSynthesizer.Sources
{
    public abstract class FunctionalAudioSourceBase : IAudioSource
    {
        public virtual AudioFormat Format { get; protected set; }

        public virtual double Position { get; protected set; }

        protected virtual float[] GetNextBuffer(int size)
        {
            return new float[size];
        }

        public abstract float[] Next(int size);
    }
}