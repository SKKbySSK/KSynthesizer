using System;
using System.Collections.Generic;
using System.Linq;
using KSynthesizer;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace TestTool.Windows
{

    public class SynthesizerWaveProvider : IWaveProvider
    {
        class Sample : ISampleProvider
        {
            public unsafe int Read(float[] buffer, int offset, int count)
            {
                if (Source == null)
                {
                    return 0;
                }

                var data = Source.Next(count);
                fixed (float* source = data)
                fixed (float* dst = buffer)
                {
                    for (int i = 0; count > i; i++)
                    {
                        dst[i] = source[i] * Volume;
                    }
                }

                return count;
            }

            public WaveFormat WaveFormat { get; } = WaveFormat.CreateIeeeFloatWaveFormat(44100, 1);

            public IAudioSource Source { get; set; }

            public float Volume { get; set; } = 1;
        }

        public SynthesizerWaveProvider()
        {
            Converter = new SampleToWaveProvider(SampleProvider);
        }

        public IAudioSource Source
        {
            get => SampleProvider.Source;
            set => SampleProvider.Source = value;
        }

        private Sample SampleProvider { get; } = new Sample();

        private SampleToWaveProvider Converter { get; }

        public float Volume
        {
            get => SampleProvider.Volume;
            set => SampleProvider.Volume = value;
        }
        
        public int Read(byte[] buffer, int offset, int count)
        {
            return Converter.Read(buffer, offset, count);
        }

        public WaveFormat WaveFormat => Converter.WaveFormat;
    }
}