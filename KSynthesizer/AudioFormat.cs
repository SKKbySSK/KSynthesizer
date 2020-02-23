namespace KSynthesizer
{
    public class AudioFormat
    {
        public AudioFormat(int sampleRate, int channels, int bitDepth)
        {
            SampleRate = sampleRate;
            Channels = channels;
            BitDepth = bitDepth;
        }

        public int SampleRate { get; }
        
        public int Channels { get; }
        
        public int BitDepth { get; }
    }
}