using System;

namespace KSynthesizer.Sources
{
    public class SinSource : PeriodicSourceBase
    {
        public SinSource(int bufferSize, int sampleRate) : base(bufferSize)
        {
            // 1ch, float(16bit) format
            Format = new AudioFormat(sampleRate, 1, 2 * 8);
        }

        public override float[] Next()
        {
            var buffer = GetNextBuffer();
            
            double periodSec = Period / 1000;
            double sinParam = 2 * Math.PI / periodSec;
            
            for (int i = 0; BufferSize > i; i++)
            {
                double deltaTime = i / (double)Format.SampleRate;
                double time = Position + deltaTime;
                buffer[i] = (float)Math.Sin(sinParam * time);
            }

            Position += (double)BufferSize / Format.SampleRate;

            return buffer;
        }
    }
}