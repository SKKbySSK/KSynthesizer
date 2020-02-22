namespace KSynthesizer
{
    public interface IAudioOutput
    {
        void Write(byte[] buffer, int offset, int count);
    }
}
