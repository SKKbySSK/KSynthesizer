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
        }

        public IReadOnlyList<PeriodicFunctionsSource> Oscillators => oscillators;

        public IAudioOutput Output { get; }

        public TimeSpan MinimumLatency { get; set; } = TimeSpan.FromMilliseconds(10);

        private void OutputOnFillBuffer(object sender, FillBufferEventArgs e)
        {
            double desiredSize = MinimumLatency.TotalSeconds * e.Format.SampleRate * e.Format.Channels;
            int length = (int)Math.Min(e.MinimumSize, desiredSize);
            int channelLen = length / e.Format.Channels;
            var channelBuffer = mixer.Next(channelLen);
            var buffer = new float[length];

            for (int i = 0; e.Format.Channels > i; i++)
            {
                Array.Copy(channelBuffer, 0, buffer, i * channelLen, channelLen);
            }
            
            e.Configure(buffer);
        }

        public void Dispose()
        {
            Output.FillBuffer -= OutputOnFillBuffer;
            Output.Dispose();
        }
    }
}