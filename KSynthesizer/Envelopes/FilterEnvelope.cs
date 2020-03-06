using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using KSynthesizer.Sources;

namespace KSynthesizer.Envelopes
{
    public class FilterEnvelope : EnvelopeFilterBase
    {
        public FilterEnvelope(PeriodicFunctionsSource source) : base(source)
        {
            Source = source;
        }

        public new PeriodicFunctionsSource Source { get; }

        protected override void ApplyAttack(ref float value, double seconds)
        {
        }

        protected override void ApplyDecay(ref float value, double seconds)
        {
        }

        protected override void ApplySustain(ref float value)
        {
        }

        protected override void ApplyRelease(ref float value, double seconds)
        {
        }

        protected override void ApplySilent(ref float value)
        {

        }
    }
}
