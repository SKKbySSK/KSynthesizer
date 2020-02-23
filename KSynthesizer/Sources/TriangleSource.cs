using System;
using System.Collections.Generic;
using System.Text;

namespace KSynthesizer.Sources
{
    public class TriangleSource : PeriodicSourceBase
    {
        public TriangleSource(int sampleRate)
        {
            // 1ch, float(32bit) format
            Format = new AudioFormat(sampleRate, 1, 32);
        }

        protected override void GenerateNextBufferForPeriod(float[] buffer)
        {
            float flen = (buffer.Length - 1f) / 2f;
            for (int i = 0; buffer.Length > i; i++)
            {
                if (i < flen)
                {
                    buffer[i] = 2 * (i / flen) - 1;
                }
                else
                {
                    buffer[i] = -2 * ((i - flen) / flen) + 1;
                }
            }
        }
    }
}
