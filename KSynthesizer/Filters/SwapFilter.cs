using System;
using System.Collections.Generic;
using System.Text;

namespace KSynthesizer.Filters
{
    public class SwapFilter<T> : IAudioFilter where T : class, IAudioSource
    {
        private AudioFormat defaultFormat = null;
        private IAudioSource currentSource = null;
        private T outputSource = null;

        public SwapFilter(Func<IAudioSource, T> onGenerate, Action<T> onDispose, AudioFormat defaultFormat = null)
        {
            OnGenerate = onGenerate;
            OnDispose = onDispose;
            this.defaultFormat = defaultFormat ?? new AudioFormat(44100, 2, 16);
        }


        public bool CopySource
        {
            get => false;
            set { }
        }

        private Func<IAudioSource, T> OnGenerate { get; }

        private Action<T> OnDispose { get; }

        public IAudioSource Source
        {
            get => currentSource;
            set
            {
                var output = outputSource;
                outputSource = null;
                if (output != null)
                {
                    OnDispose?.Invoke(outputSource);
                }

                currentSource = value;

                if (value != null)
                {
                    outputSource = OnGenerate(value);
                }
            }
        }

        public T GeneratedSource
        {
            get => outputSource;
        }

        public AudioFormat Format => outputSource?.Format ?? defaultFormat;

        public float[] Next(int size)
        {
            var source = outputSource;
            if (source != null)
            {
                return source.Next(size);
            }
            else
            {
                return new float[size];
            }
        }
    }
}
