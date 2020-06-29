using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Text;

namespace KSynthesizer.NAudio
{
    public class WasapiOutput : IAudioOutput, ISampleProvider
    {
        private WasapiOut wasapiOut;
        private bool _stopping = false;

        public WasapiOutput()
        {
        }

        public void Initialize(MMDevice device, AudioClientShareMode shareMode, TimeSpan latency, AudioFormat format)
        {
            WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(format.SampleRate, format.Channels);
            wasapiOut = new WasapiOut(device, shareMode, true, (int)latency.TotalMilliseconds);
            wasapiOut.Init(this);
            Format = new AudioFormat(wasapiOut.OutputWaveFormat.SampleRate, wasapiOut.OutputWaveFormat.Channels, wasapiOut.OutputWaveFormat.BitsPerSample);
        }

        public AudioFormat Format { get; private set; }

        public WaveFormat WaveFormat { get; private set; }

        public TimeSpan DesiredLatency { get; set; } = TimeSpan.FromMilliseconds(100);

        public TimeSpan ActualLatency => DesiredLatency;

        public bool IsRunning => (wasapiOut?.PlaybackState ?? PlaybackState.Stopped) == PlaybackState.Playing;

        public IAudioSource Source { get; set; }

        public void Dispose()
        {
            Stop();
            wasapiOut.Dispose();
            wasapiOut = null;
        }

        public void Play()
        {
            wasapiOut.Play();
        }

        public void Stop()
        {
            _stopping = true;
            wasapiOut.Stop();
            _stopping = false;
        }

        public unsafe int Read(float[] buffer, int offset, int count)
        {
            if (_stopping)
            {
                return 0;
            }

            var source = Source;
            float[] samples;
            if (source != null)
            {
                samples = source.Next(count);
            }
            else
            {
                samples = new float[count];
            }

            fixed(float* samplePtr = samples)
            fixed(float* bufferPtr = buffer)
            {
                for (int i = 0; i < count; i++)
                {
                    bufferPtr[i + offset] = samplePtr[i];
                }
            }

            return count;
        }
    }
}
