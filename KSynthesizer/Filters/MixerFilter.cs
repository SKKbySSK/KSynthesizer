using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KSynthesizer.Filters
{
    public enum MixerMode
    {
        Average,
        Trim,
    }

    public class MixerFilter : IAudioSource
    {
        public AudioFormat Format => Sources.FirstOrDefault()?.Format ?? new AudioFormat(44100, 1, 32);

        public List<IAudioSource> Sources { get; } = new List<IAudioSource>();

        public MixerMode Mode { get; set; } = MixerMode.Average;

        public float TrimVolume { get; set; } = 0.8f;

        public float Volume { get; set; } = 1;
        
        public unsafe virtual float[] Next(int size)
        {
            if (Sources.Count == 1 && Mode == MixerMode.Average)
            {
                return Sources[0].Next(size);
            }

            var buffer = new float[size];
            if (Sources.Count == 0)
            {
                return buffer;
            }

            float val;
            fixed (float* buf = buffer)
            {
                foreach (var source in Sources)
                {
                    var sourceBuffer = source.Next(size);
                    for (int i = 0; size > i; i++)
                    {
                        fixed (float* srcBuf = sourceBuffer)
                        {
                            buf[i] += srcBuf[i];
                        }
                    }
                }

                int count = Sources.Count;
                for (int i = 0; size > i; i++)
                {
                    val = buf[i];
                    switch(Mode)
                    {
                        case MixerMode.Average:
                            val = val * Volume / count;
                            break;
                        case MixerMode.Trim:
                            val = Math.Min(1, Math.Max(-1, val * TrimVolume));
                            break;
                    }
                    buf[i] = val;
                }
            }

            return buffer;
        }
    }
}
