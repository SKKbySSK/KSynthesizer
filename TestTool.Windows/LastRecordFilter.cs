using KSynthesizer;
using KSynthesizer.Filters;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestTool.Windows
{
    class LastRecordFilter<T> : IAudioSource, ILastBufferRecord where T : IAudioSource
    {
        private List<float> lastBuffer = new List<float>();
        public LastRecordFilter(T source)
        {
            Source = source;
        }

        public T Source { get; }

        public AudioFormat Format => Source?.Format ?? new AudioFormat(44100, 1, 32);

        public float[] LastBuffer => lastBuffer.ToArray();

        public int BufferLength { get; set; } = 1024;

        public float[] Next(int size)
        {
            var buffer = Source?.Next(size) ?? new float[size];
            lastBuffer.AddRange(buffer);

            if (lastBuffer.Count > BufferLength)
            {
                lastBuffer.RemoveRange(0, lastBuffer.Count - BufferLength);
            }

            return buffer;
        }
    }
}
