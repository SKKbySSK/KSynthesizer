using KSynthesizer;
using KSynthesizer.Filters;
using System;
using System.Collections.Generic;
using System.Text;

namespace Synthesizer.Windows
{
    public class BufferRecorder
    {
        public BufferRecorder(AudioFormat format)
        {
            Format = format;
        }

        public AudioFormat Format { get; }

        private List<float> lastBuffer = new List<float>();
        public float[] LastBuffer => lastBuffer.ToArray();

        public int BufferLength { get; set; } = 1024;

        public void Append(float[] buffer)
        {
            lastBuffer.AddRange(buffer);

            if (lastBuffer.Count > BufferLength)
            {
                lastBuffer.RemoveRange(0, lastBuffer.Count - BufferLength);
            }
        }
    }
}
