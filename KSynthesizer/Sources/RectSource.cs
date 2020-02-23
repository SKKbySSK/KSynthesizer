using System;
using System.Collections.Generic;
using System.Text;

namespace KSynthesizer.Sources
{
    public class RectSource : PeriodicSourceBase
    {
        public RectSource(int sampleRate)
        {
            // 1ch, float(32bit) format
            Format = new AudioFormat(sampleRate, 1, 32);
        }

        public float Duty { get; set; } = 0.5f;

        protected override void GenerateNextBufferForPeriod(float[] buffer)
        {
            for (int i = 0; buffer.Length > i; i++)
            {
                if ((i + 1) / (double)buffer.Length <= Duty)
                {
                    buffer[i] = 1;
                }
                else
                {
                    buffer[i] = -1;
                }
            }
        }
    }
}
