using System;

namespace KSynthesizer.Sources
{
    public class SinSource : PeriodicSourceBase
    {
        public SinSource(int sampleRate)
        {
            // 1ch, float(32bit) format
            Format = new AudioFormat(sampleRate, 1, 32);
        }

        protected override void GenerateNextBufferForPeriod(float[] buffer)
        {
            double periodSec = Period / 1000;
            double sinParam = 2 * Math.PI / periodSec;

            for (int i = 0; buffer.Length > i; i++)
            {
                double deltaTime = i / (double)Format.SampleRate;
                buffer[i] = (float)Math.Sin(sinParam * deltaTime);
            }
        }
    }
}