using System;
using System.Collections.Generic;
using System.Text;

namespace KSynthesizer.Envelopes
{
    public interface IEnvelopeFilter : IAudioFilter
    {
        public TimeSpan AttackDuration { get; set; }

        public TimeSpan DecayDuration { get; set; }

        public float Sustain { get; set; }

        public TimeSpan ReleaseDuration { get; set; }

        public EnvelopeState CurrentState { get; }

        public IEnvelopeAlgorithm Algorithm { get; set; }

        public void Attack();

        public void Release();
    }

    public abstract class EnvelopeFilterBase : IEnvelopeFilter
    {
        private TimeSpan currentTime = TimeSpan.Zero;
        private TimeSpan releaseTime;
        private float lastVolume;

        protected EnvelopeFilterBase(IAudioSource source)
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

        public void Attack()
        {
            currentTime = TimeSpan.Zero;
            releaseLastTime = false;
            releaseTime = TimeSpan.Zero;

            CurrentState = EnvelopeState.Attack;
        }

        public void Release()
        {
            CurrentState = EnvelopeState.Release;
        }

        protected abstract void ApplyAttack(ref float value, double seconds);
        protected abstract void ApplyDecay(ref float value, double seconds);
        protected abstract void ApplySustain(ref float value);
        protected abstract void ApplyRelease(ref float value, double seconds);
        protected abstract void ApplySilent(ref float value);

        public float[] Next(int size)
        {
            var buffer = Source.Next(size);

            if (CopySource)
            {
                var copied = new float[buffer.Length];
                Array.Copy(buffer, 0, copied, 0, buffer.Length);
                buffer = copied;
            }

            var deltaSec = buffer.Length / (double)Format.SampleRate;
            var perSec = deltaSec / buffer.Length;
            double time;
            float value;
            bool release = CurrentState == EnvelopeState.Release;

            for (int i = 0; buffer.Length > i; i++)
            {
                time = currentTime.TotalSeconds + ((i + 1) * perSec);
                value = buffer[i];
                ProcessSample(ref value, time);
                buffer[i] = value;
            }

            if (CurrentState != EnvelopeState.Silent)
            {
                currentTime += TimeSpan.FromSeconds(deltaSec);
            }

            return buffer;
        }

        private bool releaseLastTime = false;
        private void ProcessSample(ref float value, double sampleTime)
        {
            if (CurrentState == EnvelopeState.Silent)
            {
                ApplySilent(ref value);
            }
            else if (sampleTime <= AttackDuration.TotalSeconds && CurrentState == EnvelopeState.Attack)
            {
                ApplyAttack(ref value, sampleTime);
            }
            else if (sampleTime <= (AttackDuration + DecayDuration).TotalSeconds && CurrentState != EnvelopeState.Release)
            {
                ApplyDecay(ref value, sampleTime - AttackDuration.TotalSeconds);
            }
            else if (sampleTime > (AttackDuration + DecayDuration).TotalSeconds && CurrentState != EnvelopeState.Release)
            {
                ApplySustain(ref value);
            }
            else
            {
                if (CurrentState == EnvelopeState.Release && !releaseLastTime)
                {
                    releaseTime = TimeSpan.FromSeconds(sampleTime);
                    releaseLastTime = true;
                }

                if (sampleTime >= ReleaseDuration.TotalSeconds)
                {
                    value = 0;
                    CurrentState = EnvelopeState.Silent;
                    Released?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    ApplyRelease(ref value, sampleTime - releaseTime.TotalSeconds);
                }
            }
        }
    }
}
