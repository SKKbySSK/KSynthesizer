namespace KSynthesizer
{
    public interface IAudioSource
    {
        AudioFormat Format { get; }
        
        float[] Next(int size);
    }

    public interface IAudioPeriodicSource : IAudioSource
    {
        float Period { get; }
        
        void Reset();
    }
}
