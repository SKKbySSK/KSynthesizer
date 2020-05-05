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

        public float Volume { get; set; } = 0.3f;

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

        public void ConfigureInput(int sampleRate, IEnumerable<OscillatorConfig> configurations)
        {
            Mixer.ClearSources();
            foreach (var config in configurations)
            {
                Mixer.AddSource(config.CreateSource(sampleRate));
            }
        }

        public MixerFilter Mixer { get; } = new MixerFilter();

        public IEnvelopeFilter Envelope { get; }
    }

    public class Synthesizer : IAudioSource
    {
        private readonly Dictionary<int, SynthesizerInput> inputs = new Dictionary<int, SynthesizerInput>();
        private readonly MixerFilter mixer = new MixerFilter();
        private readonly object lockObj = new object();
        private int id;
        
        public Synthesizer(int sampleRate, int inputCount)
        {
            Format = new AudioFormat(sampleRate, 1, 32);
            FrequencyFilter = new FrequencyFilter(mixer);
        }

        public TimeSpan AttackDuration { get; set; }

        public TimeSpan DecayDuration { get; set; }

        public float Sustain { get; set; }

        public TimeSpan ReleaseDuration { get; set; }

        public float MixerVolume
        {
            get => mixer.Volume;
            set => mixer.Volume = value;
        }
        
        public FrequencyFilter FrequencyFilter { get; }

        public AudioFormat Format { get; }

        public int Attack(IEnumerable<OscillatorConfig> configurations)
        {
            var input = new SynthesizerInput();
            lock(lockObj)
            {
                var id = NextInput();
                inputs[id] = input;
                input.ConfigureInput(Format.SampleRate, configurations);
                mixer.AddSource(input.Envelope);
                InitializeInput(input);
                input.Envelope.Attack();

                return id;
            }
        }

        public void Attack(IEnumerable<OscillatorConfig> configurations, int id)
        {
            lock (lockObj)
            {
                SynthesizerInput input;
                if (inputs.ContainsKey(id))
                {
                    input = inputs[id];
                }
                else
                {
                    input = new SynthesizerInput();
                    inputs[id] = input;
                    mixer.AddSource(input.Envelope);
                }
                InitializeInput(input);

                input.ConfigureInput(Format.SampleRate, configurations);
                input.Envelope.Attack();
            }
        }

        public void Release(int id)
        {
            lock(lockObj)
            {
                if (inputs.TryGetValue(id, out var input))
                {
                    input.Envelope.Released += OnEnvelopeReleased;
                    input.Envelope.Release();
                }
            }

            void OnEnvelopeReleased(object sender, EventArgs e)
            {
                var envelope = (IEnvelopeFilter)sender;
                envelope.Released -= OnEnvelopeReleased;

                lock (lockObj)
                {
                    inputs.Remove(id);
                    mixer.RemoveSource(envelope);
                }
            }
        }

        public Dictionary<int, SynthesizerInput> GetInputs()
        {
            lock(lockObj)
            {
                return new Dictionary<int, SynthesizerInput>(inputs);
            }
        }

        private int NextInput()
        {
            return id++;
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
            lock (lockObj)
            {
                return FrequencyFilter.Next(size);
            }
        }
    }
}