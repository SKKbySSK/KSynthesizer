namespace KSynthesizer
{
    public interface IAudioSource
    {
        AudioFormat Format { get; }
        
        float[] Next(int size);
    }
}
