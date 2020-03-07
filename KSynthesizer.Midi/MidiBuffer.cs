using Melanchall.DryWetMidi.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KSynthesizer.Midi
{
    public class MidiBuffer
    {
        public MidiBuffer(int sampleRate, MidiFile midi)
        {
            SecondsPerSample = 1 / (double)sampleRate;

            if (midi.TimeDivision is TicksPerQuarterNoteTimeDivision ticks)
            {
                TimeDivision = ticks;
            }
            else
            {
                throw new NotSupportedException("Midi Player only support TicksPerQuarterNoteTimeDivision");
            }

            tempoChanger = TempoChanger.FromMidi(SecondsPerSample, TimeDivision, midi);
            foreach (var chunk in midi.GetTrackChunks())
            {
                tracks.Add(new MidiTrackController(SecondsPerSample, TimeDivision, chunk.Events.ToList()));
            }

            foreach(var channel in midi.GetChannels())
            {
                Channels.Add(channel);
            }
        }

        private readonly List<MidiTrackController> tracks = new List<MidiTrackController>();

        private readonly TempoChanger tempoChanger;

        public string TrackName { get; }

        public string Copyright { get; }

        public TicksPerQuarterNoteTimeDivision TimeDivision { get; }

        public double SecondsPerSample { get; }

        public TimeSpan Position => TimeSpan.FromSeconds(tempoChanger.Time);

        public List<int> Channels { get; } = new List<int>();

        public float Next(Action<MidiEvent> handleEvent, Func<float> read, Action finished)
        {
            tempoChanger.NextSample();

            bool fin = true;
            foreach(var track in tracks)
            {
                foreach(var e in track.PopEvents(tempoChanger.CurrentTempo))
                {
                    handleEvent(e);
                }

                fin = fin && track.IsFinished;
            }

            if (fin)
            {
                finished();
            }

            return read();
        }
    }
}
