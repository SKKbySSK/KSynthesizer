using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KSynthesizer.Midi
{
    public class MidiPlayer : IAudioSource
    {
        private MidiBuffer MidiBuffer { get; set; }

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
                switch (e)
                {
                    case NoteOnEvent on:
                        var freq = CalculateFrequency(on.NoteNumber);
                        foreach(var osc in OscillatorConfigs)
                        {
                            osc.Frequency = freq;
                        }
                        Synthesizer.Attack(OscillatorConfigs);
                        break;
                    case NoteOffEvent off:
                        Synthesizer.Release(index);
                        break;
                }
            }
        }

        private float CalculateFrequency(int noteNumber)
        {
            return (float)(440 * Math.Pow(2, (noteNumber - 69) / 12.0));
        }
    }
}
