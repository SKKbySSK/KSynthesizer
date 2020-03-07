using System;
using System.Collections.Generic;
using KSynthesizer.Filters;
using KSynthesizer.Sources;
using KSynthesizer.Envelopes;

namespace KSynthesizer
{
    public class OscillatorConfig
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

        public void ConfigureInput(int sampleRate, IEnumerable<OscillatorConfig> configurations)
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

    public class Synthesizer : IAudioSource
    {
        private readonly SynthesizerInput[] inputs;
        private readonly MixerFilter mixer = new MixerFilter();
        private readonly object lockObj = new object();
        private int index;
        
        public Synthesizer(int sampleRate, int inputCount)
        {
            Format = new AudioFormat(sampleRate, 1, 32);
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

        public float Sustain
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

        public AudioFormat Format { get; }

        public int Attack(IEnumerable<OscillatorConfig> configurations)
        {
            var index = NextInput();
            var input = inputs[index];
            lock(lockObj)
            {
                input.ConfigureInput(Format.SampleRate, configurations);
            }
            input.Envelope.Attack();
            return index;
        }

        public void Attack(IEnumerable<OscillatorConfig> configurations, int index)
        {
            var input = inputs[index];
            lock (lockObj)
            {
                input.ConfigureInput(Format.SampleRate, configurations);
            }
            input.Envelope.Attack();
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

        public float[] Next(int size)
        {
            lock (lockObj)
            {
                return FrequencyFilter.Next(size);
            }
        }
    }
}