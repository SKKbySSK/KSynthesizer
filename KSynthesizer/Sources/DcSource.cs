using System;
using System.Collections.Generic;
using System.Text;

namespace KSynthesizer.Sources
{
    public class DcSource : PeriodicSourceBase
    {
        public DcSource(int sampleRate)
        {
            // 1ch, float(32bit) format
            Format = new AudioFormat(sampleRate, 1, 32);
        }

        public float Value { get; set; }

        protected override void GenerateNextBufferForPeriod(float[] buffer)
        {
            for (int i = 0; buffer.Length > i; i++)
            {
                buffer[i] = Value;
            }
        }
    }
}
