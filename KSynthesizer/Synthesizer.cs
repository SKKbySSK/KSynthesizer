using System;
using System.Collections.Generic;
using KSynthesizer.Filters;
using KSynthesizer.Sources;

namespace KSynthesizer
{
    public class Synthesizer : IDisposable
    {
        private readonly List<PeriodicFunctionsSource> oscillators = new List<PeriodicFunctionsSource>();
        private readonly EnvelopeGenerator[] oscEnvelopes;
        private readonly MixerFilter mixer = new MixerFilter();
        
        public Synthesizer(IAudioOutput output, int oscillatorCount)
        {
            Output = output;
            output.FillBuffer += OutputOnFillBuffer;

            oscEnvelopes = new EnvelopeGenerator[oscillatorCount];
            for (int i = 0; oscillatorCount > i; i++)
            {
                var source = new PeriodicFunctionsSource(output.Format.SampleRate);
                oscillators.Add(source);
                
                var envelope = new EnvelopeGenerator(source);
                oscEnvelopes[i] = envelope;
                
                mixer.Sources.Add(envelope);
            }

            FrequencyFilter = new FrequencyFilter(mixer);
        }

        public TimeSpan AttackDuration
        {
            get => oscEnvelopes[0].AttackDuration;
            set
            {
                foreach (var env in oscEnvelopes)
                {
                    env.AttackDuration = value;
                }
            }
        }

        public TimeSpan DecayDuration
        {
            get => oscEnvelopes[0].DecayDuration;
            set
            {
                foreach (var env in oscEnvelopes)
                {
                    env.DecayDuration = value;
                }
            }
        }

        public float SustainVolume
        {
            get => oscEnvelopes[0].SustainVolume;
            set
            {
                foreach (var env in oscEnvelopes)
                {
                    env.SustainVolume = value;
                }
            }
        }
        
        public TimeSpan ReleaseDuration
        {
            get => oscEnvelopes[0].ReleaseDuration;
            set
            {
                foreach (var env in oscEnvelopes)
                {
                    env.ReleaseDuration = value;
                }
            }
        }
        
        public FrequencyFilter FrequencyFilter { get; }

        public IReadOnlyList<PeriodicFunctionsSource> Oscillators => oscillators;

        public IAudioOutput Output { get; }

        private void OutputOnFillBuffer(object sender, FillBufferEventArgs e)
        {
            int channelLen = e.Size / e.Format.Channels;
            var channelBuffer = FrequencyFilter.Next(channelLen);
            var buffer = new float[e.Size];

            for (int i = 0; e.Format.Channels > i; i++)
            {
                Array.Copy(channelBuffer, 0, buffer, i * channelLen, channelLen);
            }
            
            e.Configure(buffer);
        }

        public void Attack(int index, float frequency)
        {
            var osc = oscillators[index];
            osc.SetFrequency(frequency);
            osc.Reset();
            
            oscEnvelopes[index].Attack();
        }

        public void Release(int index)
        {
            oscEnvelopes[index].Release();
        }

        public void Dispose()
        {
            Output.FillBuffer -= OutputOnFillBuffer;
            Output.Dispose();
        }
    }
}