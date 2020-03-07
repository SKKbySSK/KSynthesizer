using Melanchall.DryWetMidi.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace KSynthesizer.Midi.Helpers
{
    public static class TimeConverter
    {
        public static double CalculateDeltaSeconds(MidiEvent ev, long microsecondsPerQuarterNote, long ticksPerQuarterNote)
        {
            double quarterRatio = ev.DeltaTime / (double)ticksPerQuarterNote;
            double microsec = quarterRatio * microsecondsPerQuarterNote;
            return microsec * Math.Pow(10, -6);
        }
    }
}
