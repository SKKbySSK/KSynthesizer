using System;
using System.Collections.Generic;
using KSynthesizer.Filters;
using KSynthesizer.Sources;
using KSynthesizer.Envelopes;

namespace KSynthesizer
{
    public class OscillatorConfiguration
    {
        public FunctionType Function { get; set; } = FunctionType.Sin;

        public float Frequency { get; set; } = 440;
    }

    class SynthesizerInput
    {
        public SynthesizerInput()
        {
            Envelope = new VolumeEnvelope(Mixer);
        }

        public void ConfigureInput(int sampleRate, IEnumerable<OscillatorConfiguration> configurations)
        {
            Mixer.Sources.Clear();
            foreach (var config in configurations)
            {
                Mixer.Sources.Add(new PeriodicFunctionsSource(sampleRate)
                {
                    Function = config.Function,
                    Period = 1000 / config.Frequency,
                });
            }
        }

        public MixerFilter Mixer { get; } = new MixerFilter();

        public VolumeEnvelope Envelope { get; }
    }

    public class Synthesizer : IDisposable
    {
        private readonly SynthesizerInput[] inputs;
        private readonly MixerFilter mixer = new MixerFilter();
        private readonly object lockObj = new object();
        private int index;
        private Action<float[]> interceptor;
        
        public Synthesizer(IAudioOutput output, int inputCount)
        {
            Output = output;
            output.FillBuffer += OutputOnFillBuffer;

            inputs = new SynthesizerInput[inputCount];
            for (int i = 0; inputCount > i; i++)
            {
                var input = new SynthesizerInput();
                inputs[i] = input;
                mixer.Sources.Add(input.Envelope);
            }

            FrequencyFilter = new FrequencyFilter(mixer);
        }

        public TimeSpan AttackDuration
        {
            get => inputs[0].Envelope.AttackDuration;
            set
            {
                foreach (var env in inputs)
                {
                    env.Envelope.AttackDuration = value;
                }
            }
        }

        public TimeSpan DecayDuration
        {
            get => inputs[0].Envelope.DecayDuration;
            set
            {
                foreach (var env in inputs)
                {
                    env.Envelope.DecayDuration = value;
                }
            }
        }

        public float SustainVolume
        {
            get => inputs[0].Envelope.Sustain;
            set
            {
                foreach (var env in inputs)
                {
                    env.Envelope.Sustain = value;
                }
            }
        }
        
        public TimeSpan ReleaseDuration
        {
            get => inputs[0].Envelope.ReleaseDuration;
            set
            {
                foreach (var env in inputs)
                {
                    env.Envelope.ReleaseDuration = value;
                }
            }
        }

        public float MixerVolume
        {
            get => mixer.Volume;
            set => mixer.Volume = value;
        }
        
        public FrequencyFilter FrequencyFilter { get; }

        public IAudioOutput Output { get; }

        public void Intercept(Action<float[]> interceptor)
        {
            this.interceptor = interceptor;
        }

        private void OutputOnFillBuffer(object sender, FillBufferEventArgs e)
        {
            int channelLen = e.Size / e.Format.Channels;
            lock (lockObj)
            {
                var channelBuffer = FrequencyFilter.Next(channelLen);
                var buffer = new float[e.Size];

                for (int i = 0; e.Format.Channels > i; i++)
                {
                    Array.Copy(channelBuffer, 0, buffer, i * channelLen, channelLen);
                }

                interceptor?.Invoke(buffer);
                e.Configure(buffer);
            }
        }

        public int Attack(IEnumerable<OscillatorConfiguration> configurations)
        {
            var index = NextInput();
            var input = inputs[index];
            lock(lockObj)
            {
                input.ConfigureInput(Output.Format.SampleRate, configurations);
            }
            input.Envelope.Attack();
            return index;
        }

        public void Release(int index)
        {
            inputs[index].Envelope.Release();
        }

        private int NextInput()
        {
            index++;
            if (index >= inputs.Length)
            {
                index = 0;
            }

            return index;
        }

        public void Dispose()
        {
            Output.FillBuffer -= OutputOnFillBuffer;
            interceptor = null;
        }
    }
}