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
        private readonly object lockObj = new object();
        private readonly List<IAudioSource> sources = new List<IAudioSource>();

        public AudioFormat Format => Sources.FirstOrDefault()?.Format ?? new AudioFormat(44100, 1, 32);

        public IReadOnlyList<IAudioSource> Sources => sources;

        public int NumberOfSources { get; private set; }

        public MixerMode Mode { get; set; } = MixerMode.Average;

        public float TrimVolume { get; set; } = 0.3f;

        public float Volume { get; set; } = 1;
        
        public unsafe virtual float[] Next(int size)
        {
            if (NumberOfSources == 1 && Mode == MixerMode.Average)
            {
                lock (lockObj)
                {
                    return sources[0].Next(size);
                }
            }

            var buffer = new float[size];
            if (NumberOfSources == 0)
            {
                return buffer;
            }

            float val;
            fixed (float* buf = buffer)
            {
                lock (lockObj)
                {
                    for(int i = 0; NumberOfSources > i; i++)
                    {
                        var source = sources[i];
                        var sourceBuffer = source.Next(size);
                        for (int bufferIndex = 0; size > bufferIndex; bufferIndex++)
                        {
                            fixed (float* srcBuf = sourceBuffer)
                            {
                                buf[bufferIndex] += srcBuf[bufferIndex];
                            }
                        }
                    }
                }

                for (int i = 0; size > i; i++)
                {
                    val = buf[i];
                    switch(Mode)
                    {
                        case MixerMode.Average:
                            val = val * Volume / NumberOfSources;
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

        public void AddSources(IEnumerable<IAudioSource> sources)
        {
            lock(lockObj)
            {
                this.sources.AddRange(sources);
                NumberOfSources = this.sources.Count;
            }
        }

        public void AddSource(IAudioSource source)
        {
            lock (lockObj)
            {
                sources.Add(source);
                NumberOfSources = sources.Count;
            }
        }

        public void RemoveSource(IAudioSource source)
        {
            lock (lockObj)
            {
                sources.Remove(source);
                NumberOfSources = sources.Count;
            }
        }

        public void ClearSources()
        {
            lock(lockObj)
            {
                sources.Clear();
                NumberOfSources = 0;
            }
        }
    }
}
