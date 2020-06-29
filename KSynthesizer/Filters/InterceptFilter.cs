using System;
using System.Collections.Generic;
using System.Text;

namespace KSynthesizer.Filters
{
    public class InterceptedEventArgs : EventArgs
    {
        public InterceptedEventArgs(AudioFormat format, float[] buffer)
        {
            Format = format;
            Buffer = buffer;
        }

        public AudioFormat Format { get; }

        public float[] Buffer { get; }
    }

    public class InterceptFilter : AudioFilterBase
    {
        public InterceptFilter(IAudioSource source) : base(source)
        {
        }

        public event EventHandler<InterceptedEventArgs> Intercepted;

        public override bool CopySource
        {
            get => false;
            set { }
        }

        public bool Snapshot { get; set; } = false;

        protected override void ProcessBuffer(float[] buffer)
        {
            var handler = Intercepted;
            if (handler != null)
            {
                float[] intercepted = buffer;

                if (Snapshot)
                {
                    intercepted = new float[buffer.Length];
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        intercepted[i] = buffer[i];
                    }
                }

                handler(this, new InterceptedEventArgs(Format, intercepted));
            }
        }
    }
}
