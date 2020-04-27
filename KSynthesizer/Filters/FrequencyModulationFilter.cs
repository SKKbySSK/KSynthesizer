using KSynthesizer.Sources;
using System;
using System.Collections.Generic;
using System.Text;

namespace KSynthesizer.Filters
{
    public class FrequencyModulationFilter : IAudioFilter
    {
        class FmSource : IAudioSource
        {
            private FrequencyModulationFilter filter;
            public FmSource(FrequencyModulationFilter filter)
            {
                this.filter = filter;
            }

            public AudioFormat Format => filter.Format;

            public float[] Next(int size)
            {
                float sinParam = 2 * MathF.PI * filter.CareerFrequency;
                float sampleRate = Format.SampleRate;
                float deltaTime;
                float mod;
                var samples = filter.Source.Next(size);

                for (int i = 0; size > i; i++)
                {
                    deltaTime = i / sampleRate;
                    mod = filter.Beta * samples[i];
                    samples[i] = filter.CareerVolume * MathF.Sin(sinParam * deltaTime + mod);
                }

                return samples;
            }
        }

        public FrequencyModulationFilter(IAudioSource source)
        {
            Source = source;
            fm = new FmSource(this);
        }

        private FmSource fm;

        public float Beta { get; set; } = 1;

        public float CareerVolume { get; set; } = 1;

        public float CareerFrequency { get; set; } = 300;

        public bool CopySource { get; set; } = false;

        public IAudioSource Source { get; }

        public AudioFormat Format => Source.Format;

        public float[] Next(int size)
        {
            var buffer = Source.Next(size);
            if (CopySource)
            {
                var copied = new float[buffer.Length];
                Array.Copy(buffer, copied, buffer.Length);
                buffer = copied;
            }

            return fm.Next(size);
        }
    }
}
