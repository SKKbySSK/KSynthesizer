using System;
using KSynthesizer.Filters;

namespace KSynthesizer
{
    public enum EnvelopeState
    {
        Silent,
        Attack,
        Decay,
        Sustain,
        Release,
    }
    
    public class EnvelopeGenerator : IAudioFilter
    {
        public TimeSpan AttackDuration { get; set; } = TimeSpan.FromMilliseconds(300);

        public TimeSpan DecayDuration { get; set; } = TimeSpan.FromMilliseconds(500);

        public float SustainVolume { get; set; } = 0.5f;
        
        public TimeSpan ReleaseDuration { get; set; } = TimeSpan.FromMilliseconds(1200);

        public EnvelopeState CurrentState { get; private set; } = EnvelopeState.Silent;
        
        public TimeSpan CurrentTime { get; private set; } = TimeSpan.Zero;

        public AudioFormat Format => Source.Format;

        public bool CopySource { get; set; } = false;

        private TimeSpan ReleaseTime;

        private float LastVolume;
        
        public IAudioSource Source { get; }

        public void Attack()
        {
            CurrentTime = TimeSpan.Zero;
            CurrentState = EnvelopeState.Attack;
        }

        public void Release()
        {
            ReleaseTime = CurrentTime;
            CurrentState = EnvelopeState.Release;
        }

        private void ApplyAttack(ref float value, double seconds)
        {
            float mul = (float) (seconds / AttackDuration.TotalSeconds);
            LastVolume = mul;
            value *= mul;
        }

        private void ApplyDecay(ref float value, double seconds)
        {
            float vol = 1f - SustainVolume;
            float mul = 1f - (float) (seconds / DecayDuration.TotalSeconds) * vol;
            LastVolume = mul;
            value *= mul;
        }

        private void ApplySustain(ref float value)
        {
            LastVolume = SustainVolume;
            value *= SustainVolume;
        }

        private bool ApplyRelease(ref float value, double seconds)
        {
            float mul = (1f - (float) (seconds / ReleaseDuration.TotalSeconds)) * LastVolume;
            value *= mul;

            return seconds >= ReleaseDuration.TotalSeconds;
        }

        public EnvelopeGenerator(IAudioSource source)
        {
            Source = source;
        }
        
        public float[] Next(int size)
        {
            if (CurrentState == EnvelopeState.Silent)
            {
                return new float[size];
            }

            var buffer = Source.Next(size);
            var deltaSec = buffer.Length / (double) Format.SampleRate;
            var perSec = deltaSec / buffer.Length;
            double time;
            float value;
            for (int i = 0; buffer.Length > i; i++)
            {
                time = CurrentTime.TotalSeconds + ((i + 1) * perSec);
                value = buffer[i];

                if (time <= AttackDuration.TotalSeconds && CurrentState == EnvelopeState.Attack)
                {
                    ApplyAttack(ref value, time);
                } 
                else if (time <= (AttackDuration + DecayDuration).TotalSeconds && CurrentState != EnvelopeState.Release)
                {
                    ApplyDecay(ref value, time - AttackDuration.TotalSeconds);
                }
                else if (CurrentState != EnvelopeState.Release)
                {
                    ApplySustain(ref value);
                }
                else
                {
                    if (ApplyRelease(ref value, time - ReleaseTime.TotalSeconds))
                    {
                        CurrentState = EnvelopeState.Silent;
                        if (Source is IAudioPeriodicSource periodic)
                        {
                            periodic.Reset();
                        }
                    }
                }

                buffer[i] = value;
            }

            if (CurrentState != EnvelopeState.Silent)
            {
                CurrentTime += TimeSpan.FromSeconds(deltaSec);
            }

            return buffer;
        }
    }
}