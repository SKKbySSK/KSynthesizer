using System;
using System.Collections.Generic;
using System.Text;

namespace KSynthesizer.Sources
{
    public class SawtoothSource : PeriodicSourceBase
    {
        public SawtoothSource(int sampleRate)
        {
            // 1ch, float(32bit) format
            Format = new AudioFormat(sampleRate, 1, 32);
        }

        protected override void GenerateNextBufferForPeriod(float[] buffer)
        {
            float flen = buffer.Length - 1f;
            for (int i = 0; buffer.Length > i; i++)
            {
                buffer[i] = 2 * (i / flen) - 1;
            }
        }
    }
}
