namespace KSynthesizer
{
    public interface IAudioSource
    {
        int BufferSize { get; }
        
        AudioFormat Format { get; }
        
        float[] Next();
    }
}
