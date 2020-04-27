using System;
using System.Collections.Generic;
using System.Text;

namespace KSynthesizer.Envelopes
{
    public class VolumeEnvelope2 : IEnvelopeFilter
    {
        public VolumeEnvelope2(IAudioSource source)
        {
            Source = source;
        }

        public event EventHandler Released;

        public TimeSpan AttackDuration { get; set; } = TimeSpan.FromMilliseconds(300);

        public TimeSpan DecayDuration { get; set; } = TimeSpan.FromMilliseconds(500);

        public float Sustain { get; set; } = 0.5f;

        public TimeSpan ReleaseDuration { get; set; } = TimeSpan.FromMilliseconds(3000);

        public EnvelopeState CurrentState { get; private set; } = EnvelopeState.Silent;

        public IEnvelopeAlgorithm Algorithm { get; set; } = new LinearEnvelopeAlgorithm();

        public AudioFormat Format => Source.Format;

        public bool CopySource { get; set; } = false;

        public IAudioSource Source { get; }

        private EnvelopeGenerator Generator { get; } = new EnvelopeGenerator();

        public void Attack()
        {
            Generator.AttackRate = CalcRate(AttackDuration);
            Generator.DecayRate = CalcRate(DecayDuration);
            Generator.SustainLevel = Sustain;
            Generator.ReleaseRate = CalcRate(ReleaseDuration);

            Generator.Gate(true);
        }

        public void Release()
        {
            Generator.AttackRate = CalcRate(AttackDuration);
            Generator.DecayRate = CalcRate(DecayDuration);
            Generator.SustainLevel = Sustain;
            Generator.ReleaseRate = CalcRate(ReleaseDuration);

            Generator.Gate(false);
        }

        private float CalcRate(TimeSpan duration)
        {
            return (float)(duration.TotalSeconds * Format.SampleRate);
        }

        public float[] Next(int size)
        {
            var buffer = Source.Next(size);
            var isIdle = Generator.State == EnvelopeGenerator.EnvelopeState.Idle;

            for (int i = 0; buffer.Length > i; i++)
            {
                buffer[i] *= Generator.Process();

                if (Generator.State == EnvelopeGenerator.EnvelopeState.Idle && !isIdle)
                {
                    Released?.Invoke(this, EventArgs.Empty);
                    isIdle = true;
                }
            }

            return buffer;
        }
    }
}
