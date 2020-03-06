using System;
using System.Collections.Generic;
using System.Text;

namespace KSynthesizer
{
    public enum IonianScale
    {
        A = 0,
        B = 1,
        C = 2,
        D = 3,
        E = 4,
        F = 5,
        G = 6,
    }

    public struct IonianTone
    {
        public IonianScale Scale { get; set; }

        public bool Sharp { get; set; }

        public float Octave { get; set; }

        public int Semitones { get; set; }
    }

    public static class Scale
    {
        public static float GetFrequency(IonianScale scale, bool sharp, float octave, int semitones = 0)
        {
            double freq = 27.5 * Math.Pow(2, octave);
            int scaleTimes = semitones + (int)scale * 2;

            switch (scale)
            {
                case IonianScale.C:
                case IonianScale.D:
                case IonianScale.E:
                    scaleTimes--;
                    break;
                case IonianScale.F:
                case IonianScale.G:
                    scaleTimes -= 2;
                    break;
            }

            if (sharp)
            {
                scaleTimes++;
            }

            freq *= Math.Pow(Math.Pow(2, 1.0 / 12.0), scaleTimes);

            return (float)freq;
        }

        public static float GetFrequency(IonianTone tone)
        {
            return GetFrequency(tone.Scale, tone.Sharp, tone.Octave, tone.Semitones);
        }
    }
}
