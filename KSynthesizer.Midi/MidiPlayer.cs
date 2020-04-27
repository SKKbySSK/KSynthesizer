using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KSynthesizer.Midi
{
    public class MidiEventArgs: EventArgs
    {
        public MidiEventArgs(bool processed, MidiEvent e)
        {
            Processed = processed;
            Event = e;
        }

        public bool Processed { get; }

        public MidiEvent Event { get; }
    }

    public class MidiPlayer : IAudioSource
    {
        private List<int> sustains = new List<int>();
        private bool sustainPedal = false;

        private MidiBuffer MidiBuffer { get; set; }

        public event EventHandler<MidiEventArgs> EventReceived;

        public event EventHandler Finished;

        public MidiPlayer(int sampleRate, MidiFile midi)
        {
            Format = new AudioFormat(sampleRate, 1, 32);
            MidiBuffer = new MidiBuffer(Format.SampleRate, midi);
            Synthesizer = new Synthesizer(Format.SampleRate, MidiBuffer.Channels.Count);
        }

        public AudioFormat Format { get; }

        public TimeSpan Position { get; private set; } = TimeSpan.Zero;

        public Synthesizer Synthesizer { get; }

        public TimeSpan SustainPedalDuration { get; set; } = TimeSpan.FromMilliseconds(200);

        public List<OscillatorConfig> OscillatorConfigs { get; } = new List<OscillatorConfig>();

        public float[] Next(int size)
        {
            var buffer = new float[size];
            for (int i = 0; size > i; i++)
            {
                buffer[i] = MidiBuffer.Next(HandleEvent, ReadNext, OnFinished);
            }

            return buffer;
        }

        private void OnFinished()
        {
            Finished?.Invoke(this, EventArgs.Empty);
        }

        private float ReadNext()
        {
            return Synthesizer.Next(1)[0];
        }

        private void HandleEvent(MidiEvent e)
        {
            if (e is ChannelEvent channelEvent)
            {
                int channel = channelEvent.Channel;
                int index = MidiBuffer.Channels.IndexOf(channel);
                bool processed = false;
                switch (e)
                {
                    case NoteOnEvent on:
                        if (sustainPedal && !sustains.Contains(index))
                        {
                            sustains.Add(index);
                        }
                        var freq = CalculateFrequency(on.NoteNumber);
                        foreach(var osc in OscillatorConfigs)
                        {
                            osc.Frequency = freq;
                        }
                        Synthesizer.Attack(OscillatorConfigs, index);
                        processed = true;
                        break;
                    case NoteOffEvent off:
                        if (!sustainPedal)
                        {
                            Synthesizer.Release(index);
                        }
                        processed = true;
                        break;
                    case ControlChangeEvent controlChange:
                        processed = HandleControlChange(controlChange);
                        break;
                }

                EventReceived?.Invoke(this, new MidiEventArgs(processed, e));
            }
        }

        private bool HandleControlChange(ControlChangeEvent e)
        {
            switch (e.ControlNumber)
            {
                case 64: //Sustain Pedal
                    if (e.ControlValue >= 64)
                    {
                        sustains.Clear();
                        sustainPedal = true;
                    }
                    else
                    {
                        sustainPedal = false;
                        var inputs = Synthesizer.GetInputs();
                        foreach(var input in inputs)
                        {
                            if (sustains.Contains(input.Key))
                            {
                                input.Value.Envelope.ReleaseDuration = SustainPedalDuration;
                                Synthesizer.Release(input.Key);
                            }
                        }
                        sustains.Clear();
                    }
                    return true;
            }

            return false;
        }

        private float CalculateFrequency(int noteNumber)
        {
            return (float)(440 * Math.Pow(2, (noteNumber - 69) / 12.0));
        }
    }
}
