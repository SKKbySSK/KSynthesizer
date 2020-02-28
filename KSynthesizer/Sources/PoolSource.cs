using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KSynthesizer.Sources
{
    public class PoolSource : IAudioSource
    {
        private readonly IAudioSource source;
        private readonly BufferSink<float> sink;
        private readonly TimeSpan defaultDelay;
        private Task currentFillTask;

        public PoolSource(IAudioSource source, TimeSpan delay)
        {
            this.source = source;
            defaultDelay = delay;
            int size = (int)(source.Format.SampleRate * source.Format.Channels * delay.TotalSeconds) * 3;
            sink = new BufferSink<float>(size);
            FillSink();
        }

        public AudioFormat Format => source.Format;

        private TimeSpan ActualDelay
        {
            get
            {
                return TimeSpan.FromSeconds(Format.SampleRate * Format.Channels / (double)sink.Size);
            }
        }

        public float[] Next(int size)
        {
            if (size * 3 > sink.Size)
            {
                sink.Size = size * 3;
            }

            Task t = null;
            lock (sink)
            {
                if (!sink.IsFilled)
                {
                    FillSink();
                    t = currentFillTask;
                }
            }

            if (t != null)
            {
                t.Wait();
            }

            float[] buffer = null;
            lock (sink)
            {
                buffer = sink.Pop(size);
            }

            FillSink();
            return buffer;
        }

        private async void FillSink()
        {
            var t = currentFillTask;
            if (t != null)
            {
                await t;
            }

            lock (sink)
            {
                if (sink.IsFilled)
                {
                    return;
                }
            }

            currentFillTask = Task.Run(() =>
            {
                var buffer = source.Next(sink.Size);

                lock (sink)
                {
                    sink.Push(buffer, false);
                }

                currentFillTask = null;
            });
        }
    }
}
