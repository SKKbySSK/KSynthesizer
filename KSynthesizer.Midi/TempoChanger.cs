using Melanchall.DryWetMidi.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace KSynthesizer.Midi
{
    class TempoChanger
    {
        private readonly List<SetTempoEvent> tempos;
        private int tempoIndex = 0;

        public static TempoChanger FromMidi(double secondsPerPop, TicksPerQuarterNoteTimeDivision timeDivision, MidiFile midi)
        {
            var tempos = new List<SetTempoEvent>();
            foreach (var chunk in midi.GetTrackChunks())
            {
                long time = 0;
                foreach (var ev in chunk.Events)
                {
                    time += ev.DeltaTime;
                    if (ev is SetTempoEvent tempo)
                    {
                        tempos.Add(new SetTempoEvent()
                        {
                            DeltaTime = time,
                            MicrosecondsPerQuarterNote = tempo.MicrosecondsPerQuarterNote,
                        });
                    }
                }
            }
            tempos.Sort((x, y) => x.DeltaTime.CompareTo(y.DeltaTime));

            return new TempoChanger(secondsPerPop, timeDivision, tempos);
        }

        private TempoChanger(double secondsPerPop, TicksPerQuarterNoteTimeDivision timeDivision, List<SetTempoEvent> tempoEvents)
        {
            SecondsPerPop = secondsPerPop;
            TimeDivision = timeDivision;
            tempos = tempoEvents;

            if (tempoEvents.Count == 0)
            {
                throw new ArgumentException("There is no tempo event");
            }
            CurrentTempo = tempoEvents[0];
        }

        public double SecondsPerPop { get; }

        public double Time { get; private set; } = 0;

        public TicksPerQuarterNoteTimeDivision TimeDivision { get; }

        public SetTempoEvent CurrentTempo { get; private set; }

        public void Reset()
        {
            CurrentTempo = tempos[0];
            Time = 0;
            tempoIndex = 0;
        }

        public void NextSample()
        {
            var lastTempo = tempos[tempoIndex];
            var nextTime = Time + SecondsPerPop;
            while(tempoIndex + 1 < tempos.Count)
            {
                var tempo = tempos[tempoIndex + 1];
                var tempoTime = Helpers.TimeConverter.CalculateDeltaSeconds(tempo, lastTempo.MicrosecondsPerQuarterNote, TimeDivision.TicksPerQuarterNote);
                if (tempoTime <= nextTime)
                {
                    lastTempo = tempo;
                    CurrentTempo = tempo;
                    tempoIndex++;
                }
                else
                {
                    break;
                }
            }

            Time = nextTime;
        }
    }
}
