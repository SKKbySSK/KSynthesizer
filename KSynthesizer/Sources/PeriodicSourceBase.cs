namespace KSynthesizer.Sources
{
    public abstract class PeriodicSourceBase : FunctionalAudioSourceBase
    {
        protected PeriodicSourceBase(int bufferSize) : base(bufferSize)
        {
        }

        /// <summary>
        /// Period in millisecond
        /// </summary>
        public float Period { get; set; } = 1000;

        public void SetFrequency(float frequency)
        {
            Period = 1000 / frequency;
        }
    }
}