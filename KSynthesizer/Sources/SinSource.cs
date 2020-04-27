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
            float periodSec = Period / 1000;
            float sinParam = 2 * MathF.PI / periodSec;
            float sampleRate = Format.SampleRate;
            float deltaTime;

            for (int i = 0; buffer.Length > i; i++)
            {
                deltaTime = i / sampleRate;
                buffer[i] = MathF.Sin(sinParam * deltaTime);
            }
        }
    }
}
