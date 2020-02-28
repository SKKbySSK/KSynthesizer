using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace KSynthesizer.Filters
{
    public enum FrequencyFilterMode
    {
        Disable,
        Lowpass,
        Highpass,
        Bandpass,
    }

    public class FrequencyFilter : AudioFilterBase
    {
        private BiQuadFilter filter;

        public FrequencyFilter(IAudioSource source) : base(source)
        {
        }

        public FrequencyFilterMode Mode { get; private set; } = FrequencyFilterMode.Disable;

        public float CutoffLow { get; private set; }

        public float CutoffHigh { get; private set; }

        public void SetLowpassMode(float cutoff)
        {
            Mode = FrequencyFilterMode.Lowpass;
            CutoffLow = cutoff;
            filter = BiQuadFilter.LowPassFilter(Format.SampleRate, cutoff, 1);
        }

        public void SetHighpassMode(float cutoff)
        {
            Mode = FrequencyFilterMode.Highpass;
            CutoffHigh = cutoff;
            filter = BiQuadFilter.HighPassFilter(Format.SampleRate, cutoff, 1);
        }

        public void SetBandpassMode(float cutoffLow, float cutoffHigh)
        {
            Mode = FrequencyFilterMode.Bandpass;
            CutoffLow = cutoffLow;
            CutoffHigh = cutoffHigh;
            float center = cutoffLow + ((cutoffHigh - cutoffLow) / 2);
            float q = center / (cutoffHigh - cutoffLow);
            filter = BiQuadFilter.BandPassFilterConstantSkirtGain(Format.SampleRate, center, q);
        }

        public void ChangeFrequency(float cutoffLow, float cutoffHigh)
        {
            switch (Mode)
            {
                case FrequencyFilterMode.Lowpass:
                    SetLowpassMode(cutoffLow);
                    break;
                case FrequencyFilterMode.Highpass:
                    SetHighpassMode(cutoffHigh);
                    break;
                case FrequencyFilterMode.Bandpass:
                    SetBandpassMode(cutoffLow, cutoffHigh);
                    break;
            }
        }

        public void Disable()
        {
            Mode = FrequencyFilterMode.Disable;
            filter = null;
        }

        protected override void ProcessBuffer(float[] buffer)
        {
            if (filter == null)
            {
                return;
            }

            for (int i = 0; buffer.Length > i; i++)
            {
                buffer[i] = filter.Transform(buffer[i]);
            }
        }
    }
}
