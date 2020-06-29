using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;

namespace KSynthesizer.Filters
{
    public enum MixerMode
    {
        Average,
        Sum,
    }

    public class MixerFilter : IAudioSource
    {
        private readonly List<IAudioSource> sources = new List<IAudioSource>();
        private bool modified = true;
        private IAudioSource[] readingSources;

        public AudioFormat Format => Sources.FirstOrDefault()?.Format ?? new AudioFormat(44100, 1, 32);

        public IReadOnlyList<IAudioSource> Sources => sources;

        public MixerMode Mode { get; set; } = MixerMode.Sum;

        public float Volume { get; set; } = 1;
        
        public unsafe virtual float[] Next(int size)
        {
            if (modified)
            {
                lock (((System.Collections.ICollection)sources).SyncRoot)
                {
                    readingSources = sources.ToArray();
                    modified = false;
                }
            }

            if (readingSources.Length == 1)
            {
                return readingSources[0].Next(size);
            }

            var buffer = new float[size];
            if (readingSources.Length == 0)
            {
                return buffer;
            }

            float val;
            fixed (float* buf = buffer)
            {
                for (int i = 0; readingSources.Length > i; i++)
                {
                    var source = readingSources[i];
                    var sourceBuffer = source.Next(size);
                    for (int bufferIndex = 0; size > bufferIndex; bufferIndex++)
                    {
                        fixed (float* srcBuf = sourceBuffer)
                        {
                            buf[bufferIndex] += srcBuf[bufferIndex];
                        }
                    }
                }

                for (int i = 0; size > i; i++)
                {
                    val = buf[i];
                    switch(Mode)
                    {
                        case MixerMode.Average:
                            val = val * Volume / readingSources.Length;
                            break;
                    }

                    if (val >= 1)
                    {
                        buf[i] = 1;
                    }
                    else if (val <= -1)
                    {
                        buf[i] = -1;
                    }
                    else
                    {
                        buf[i] = val;
                    }
                }
            }

            return buffer;
        }

        public void AddSources(IEnumerable<IAudioSource> sources)
        {
            lock (((System.Collections.ICollection)this.sources).SyncRoot)
            {
                this.sources.AddRange(sources);
                modified = true;
            }
        }

        public void AddSource(IAudioSource source)
        {
            lock (((System.Collections.ICollection)sources).SyncRoot)
            {
                sources.Add(source);
                modified = true;
            }
        }

        public void RemoveSource(IAudioSource source)
        {
            lock (((System.Collections.ICollection)sources).SyncRoot)
            {
                sources.Remove(source);
                modified = true;
            }
        }

        public void ClearSources()
        {
            lock (((System.Collections.ICollection)sources).SyncRoot)
            {
                sources.Clear();
                modified = true;
            }
        }
    }
}
