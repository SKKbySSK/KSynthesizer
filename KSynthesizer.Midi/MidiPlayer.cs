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
        private Dictionary<int, int> channelMap = new Dictionary<int, int>();
        private bool sustainPedal = false;

        private MidiBuffer MidiBuffer { get; set; }

        public event EventHandler<MidiEventArgs> EventReceived;

        public event EventHandler Finished;

        public MidiPlayer(int sampleRate, Synthesizer synthesizer = null)
        {
            Format = new AudioFormat(sampleRate, 1, 32);
            Synthesizer = synthesizer ?? new Synthesizer(Format.SampleRate, MidiBuffer.Channels.Count);
        }

        public void Open(MidiFile file)
        {
            MidiBuffer = new MidiBuffer(Format.SampleRate, file);

            for (int i = 0; i < MidiBuffer.Channels.Count; i++)
            {
                channelMap[MidiBuffer.Channels[i]] = i;
            }
        }

        public AudioFormat Format { get; }

        public TimeSpan Position { get; private set; } = TimeSpan.Zero;

        public Synthesizer Synthesizer { get; }

        public TimeSpan SustainPedalDuration { get; set; } = TimeSpan.FromMilliseconds(200);

        public List<Oscillator> OscillatorConfigs { get; } = new List<Oscillator>();

        public float[] Next(int size)
        {
            var buffer = new float[size];

            if (MidiBuffer == null)
            {
                return buffer;
            }

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
                        var index = channelMap[e.Channel];
                        var input = Synthesizer.GetInput(index);
                        input.Envelope.ReleaseDuration = SustainPedalDuration;
                        input.Envelope.Release();
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
