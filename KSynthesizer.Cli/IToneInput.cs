using System;
using System.Collections.Generic;
using System.Text;

namespace KSynthesizer.Cli
{
    interface IToneInput
    {
        IonianTone Tone { get; }

        bool Release { get; }

        bool Exit { get; }

        void Read();
    }

    class TestToneInput : IToneInput
    {
        private int state = 0;

        public IonianTone Tone { get; private set; }

        public bool Exit { get; private set; } = false;

        public bool Release { get; private set; } = false;

        public void Read()
        {
            System.Threading.Thread.Sleep(500);
            switch (state++)
            {
                case 10:
                    Exit = true;
                    break;
                default:
                    Tone = new IonianTone()
                    {
                        Scale = IonianScale.A,
                    };
                    Release = (state % 2 == 0);
                    break;
            }
        }
    }
}
