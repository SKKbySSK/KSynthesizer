using KSynthesizer.Soundio;
using System;
using System.Collections.Generic;
using System.Text;

namespace KSynthesizer.Cli
{
    class SynthesizerPlayer
    {
        private readonly Dictionary<IonianTone, int> toneMap = new Dictionary<IonianTone, int>();

        public SoundioOutput Output { get; } = new SoundioOutput();

        public Synthesizer Synthesizer { get; private set; }

        public float OscillatorVolume { get; set; } = 0.4f;

        public void Initialize(SoundIOSharp.SoundIODevice device, AudioFormat format)
        {
            Output.Initialize(device, format);

            Synthesizer = new Synthesizer(format.SampleRate, 10);
        }

        public void Run(IToneInput input)
        {
            while(!input.Exit)
            {
                input.Read();
                if (input.Release)
                {
                    Release(input.Tone);
                } 
                else
                {
                    Attack(input.Tone);
                }
            }
        }

        public void Attack(IonianTone tone)
        {
            var freq = Scale.GetFrequency(tone);
            var index = Synthesizer.Attack(new Oscillator[] { new Oscillator(Sources.FunctionType.Sin, freq, OscillatorVolume) });
            toneMap[tone] = index;
        }

        public void Release(IonianTone tone)
        {
           if (toneMap.TryGetValue(tone, out var index))
            {
                toneMap.Remove(tone);
                Synthesizer.Release(index);
            }
        }
    }
}
