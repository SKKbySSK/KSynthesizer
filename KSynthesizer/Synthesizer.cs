using System;
using System.Collections.Generic;
using KSynthesizer.Filters;
using KSynthesizer.Sources;
using KSynthesizer.Envelopes;
using System.Threading;

namespace KSynthesizer
{
    public class Oscillator
    {
        public FunctionType Function { get; set; } = FunctionType.Sin;

        public float Frequency { get; set; } = 440;

        public float Volume { get; set; } = 0.5f;

        protected internal virtual IAudioSource CreateSource(int sampleRate)
        {
            return new PeriodicFunctionsSource(sampleRate)
            {
                Function = Function,
                Period = 1000 / Frequency,
                Volume = Volume,
            };
        }
    }

    public class SynthesizerInput
    {
        public SynthesizerInput()
        {
            Envelope = new VolumeEnvelope2(Mixer);
        }

        public void ConfigureInput(int sampleRate, IEnumerable<Oscillator> oscillators)
        {
            Mixer.ClearSources();
            foreach (var oscillator in oscillators)
            {
                Mixer.AddSource(oscillator.CreateSource(sampleRate));
            }
        }

        public MixerFilter Mixer { get; } = new MixerFilter();

        public IEnvelopeFilter Envelope { get; }
    }

    public class Synthesizer : IAudioSource
    {
        private readonly SynthesizerInput[] inputs;
        private readonly MixerFilter mixer = new MixerFilter();
        private int index = -1;
        
        public Synthesizer(int sampleRate, int maximumInputs = 100)
        {
            inputs = new SynthesizerInput[maximumInputs];
            SynthesizerInput input;
            for (int i = 0; i < maximumInputs; i++)
            {
                input = new SynthesizerInput();
                inputs[i] = input;
                mixer.AddSource(input.Envelope);
            }

            Format = new AudioFormat(sampleRate, 1, 32);
            FrequencyFilter = new FrequencyFilter(mixer);
            FrequencyFilter.Disable();
        }

        public TimeSpan AttackDuration { get; set; } = TimeSpan.FromMilliseconds(100);

        public TimeSpan DecayDuration { get; set; } = TimeSpan.FromMilliseconds(700);

        public float Sustain { get; set; } = 0.5f;

        public TimeSpan ReleaseDuration { get; set; } = TimeSpan.FromMilliseconds(2000);

        public float MixerVolume
        {
            get => mixer.Volume;
            set => mixer.Volume = value;
        }
        
        public FrequencyFilter FrequencyFilter { get; }

        public AudioFormat Format { get; }

        public int Attack(IEnumerable<Oscillator> oscillators)
        {
            var index = NextInput();
            Attack(oscillators, index);
            return index;
        }

        public void Attack(IEnumerable<Oscillator> oscillators, int index)
        {
            ValidateIndex(index, true);
            SynthesizerInput input = inputs[index];

            input.ConfigureInput(Format.SampleRate, oscillators);
            InitializeInput(input);
            input.Envelope.Attack();
        }

        public void Release(int index)
        {
            ValidateIndex(index, true);
            inputs[index].Envelope.Release();
        }

        public SynthesizerInput GetInput(int index)
        {
            ValidateIndex(index, true);
            return inputs[index];
        }

        private int NextInput()
        {
            if (++index >= inputs.Length)
            {
                index = 0;
            }

            return index;
        }

        private bool ValidateIndex(int index, bool throwIfInvalid)
        {
            if (index >= 0 && inputs.Length > index)
            {
                return true;
            }
            else
            {
                if (throwIfInvalid)
                {
                    throw new IndexOutOfRangeException();
                }

                return false;
            }
        }

        private void InitializeInput(SynthesizerInput input)
        {
            input.Envelope.AttackDuration = AttackDuration;
            input.Envelope.DecayDuration = DecayDuration;
            input.Envelope.Sustain = Sustain;
            input.Envelope.ReleaseDuration = ReleaseDuration;
        }

        public float[] Next(int size)
        {
            return FrequencyFilter.Next(size);
        }
    }
}