using System;
using System.Collections.Generic;
using System.Text;

namespace KSynthesizer.Envelopes
{
    public interface IEnvelopeAlgorithm
    {
        float Attack(float progress);

        float Decay(float progress);

        float Release(float progress);
    }

    public class LinearEnvelopeAlgorithm : IEnvelopeAlgorithm
    {
        public float Attack(float progress)
        {
            return progress;
        }

        public float Decay(float progress)
        {
            return progress;
        }

        public float Release(float progress)
        {
            return progress;
        }
    }
}
