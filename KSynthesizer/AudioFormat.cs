namespace KSynthesizer
{
    public class AudioFormat
    {
        public AudioFormat(int sampleRate, int channels, int bitDepth)
        {
            SampleRate = sampleRate;
            Channels = channels;
            this.bitDepth = bitDepth;
        }

        public int SampleRate { get; }
        
        public int Channels { get; }
        
        public int bitDepth { get; }
    }
}