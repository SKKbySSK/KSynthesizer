using System;
using System.Collections.Generic;
using System.Text;

namespace KSynthesizer.Filters
{
    public abstract class AudioFilterBase : IAudioFilter
    {
        protected AudioFilterBase(IAudioSource source)
        {
            Source = source;
        }

        public virtual bool CopySource { get; set; } = false;

        public virtual AudioFormat Format => Source.Format;

        public virtual IAudioSource Source { get; }

        public float[] Next(int size)
        {
            var buffer = Source.Next(size);
            if (CopySource)
            {
                var copied = new float[buffer.Length];
                Array.Copy(buffer, copied, buffer.Length);
                buffer = copied;
            }

            ProcessBuffer(buffer);
            return buffer;
        }

        protected abstract void ProcessBuffer(float[] buffer);
    }
}
