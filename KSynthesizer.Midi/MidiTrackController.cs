using Melanchall.DryWetMidi.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace KSynthesizer.Midi
{
    class MidiTrackController
    {
        private readonly List<MidiEvent> events;
        private int eventIndex = -1;
        private double lastSampleTime = 0;

        public MidiTrackController(double secondsPerPop, TicksPerQuarterNoteTimeDivision timeDivision, List<MidiEvent> trackEvents)
        {
            SecondsPerPop = secondsPerPop;
            TimeDivision = timeDivision;
            events = trackEvents;
        }

        public double SecondsPerPop { get; }

        public double Time { get; private set; } = 0;

        public TicksPerQuarterNoteTimeDivision TimeDivision { get; }

        public bool IsFinished { get; private set; } = false;

        public void Reset()
        {
            lastSampleTime = 0;
            eventIndex = -1;
            Time = 0;
            IsFinished = false;
        }

        public List<MidiEvent> PopEvents(SetTempoEvent lastTempoEvent)
        {
            var res = new List<MidiEvent>();
            var nextTime = Time + SecondsPerPop;

            bool add = true;
            while(eventIndex + 1 < events.Count && add)
            {
                add = false;
                var e = events[eventIndex + 1];
                var sampleTime = lastSampleTime + Helpers.TimeConverter.CalculateDeltaSeconds(e, lastTempoEvent.MicrosecondsPerQuarterNote, TimeDivision.TicksPerQuarterNote);
                if (sampleTime <= nextTime)
                {
                    add = true;
                    res.Add(e);
                    eventIndex++;
                    lastSampleTime = sampleTime;
                }
            }

            if (eventIndex + 1 >= events.Count)
            {
                IsFinished = true;
            }

            Time = nextTime;

            return res;
        }
    }
}
