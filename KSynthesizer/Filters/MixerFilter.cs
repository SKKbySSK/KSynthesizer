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
        
        public unsafe virtual float[] Next(int size)
        {
            var buffer = new float[size];
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
                            val = buf[i] + srcBuf[i];
                            switch (Mode)
                            {
                                case MixerMode.Trim:
                                    buf[i] = Math.Min(1, Math.Max(val, -1));
                                    break;
                                default:
                                    buf[i] = val;
                                    break;
                            }
                        }
                    }
                }

                if (Mode == MixerMode.Average)
                {
                    int count = Sources.Count;
                    for (int i = 0; size > i; i++)
                    {
                        buf[i] /= count;
                    }
                }
            }

            return buffer;
        }
    }
}
