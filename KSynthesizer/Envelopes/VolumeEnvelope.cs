using System;
using KSynthesizer.Filters;

namespace KSynthesizer.Envelopes
{
    public class VolumeEnvelope : EnvelopeFilterBase
    {
        private float lastVolume;

        public VolumeEnvelope(IAudioSource source) : base(source)
        {
        }

        public float SilentLevel { get; set; } = 0f;

        protected override void ApplyAttack(ref float value, double seconds)
        {
            float mul = (float) (seconds / AttackDuration.TotalSeconds);
            mul = Algorithm.Attack(mul);
            lastVolume = Math.Max(mul, SilentLevel);
            value *= Math.Max(mul, SilentLevel);
        }

        protected override void ApplyDecay(ref float value, double seconds)
        {
            float vol = 1f - Sustain;
            float prog = (float)(seconds / DecayDuration.TotalSeconds);
            prog = Algorithm.Decay(prog);
            float mul = 1f - prog * vol;
            lastVolume = Math.Max(mul, SilentLevel);
            value *= Math.Max(mul, SilentLevel);
        }

        protected override void ApplySustain(ref float value)
        {
            lastVolume = Math.Max(Sustain, SilentLevel);
            value *= Math.Max(Sustain, SilentLevel); ;
        }

        protected override void ApplyRelease(ref float value, double seconds)
        {
            float prog = (float)(seconds / ReleaseDuration.TotalSeconds);
            prog = Algorithm.Release(prog);
            float mul = lastVolume * (1f - prog);
            value *= Math.Max(mul, SilentLevel);
        }

        protected override void ApplySilent(ref float value)
        {
            value *= SilentLevel;
        }
    }
}