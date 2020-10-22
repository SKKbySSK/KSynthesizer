using System;
using System.Collections.Generic;
using SoundIOSharp;

namespace KSynthesizer.Soundio
{
    public class OutputBufferEventArgs : EventArgs
    {
        public OutputBufferEventArgs(int size)
        {
            Size = size;
        }

        public int Size { get; }
        
        public float[] Buffer { get; set; }
    }
    
    public class SoundioOutput : IAudioOutput
    {
        private readonly SoundIO api = new SoundIO();
        private RingBuffer<float> ringBuffer;
        private SoundIOOutStream outstream;

        public SoundioOutput()
        {
            api.Connect();
            api.FlushEvents();
            
            for (int i = 0; api.OutputDeviceCount > i; i++)
            {
                var device = api.GetOutputDevice(i);
                if (i == api.DefaultOutputDeviceIndex)
                {
                    DefaultDevice = device;
                }

                Devices.Add(device);
            }
        }

        public event EventHandler Underflow;
        
        public event EventHandler<OutputBufferEventArgs> Buffer;
        
        public bool IsRunning { get; private set; }

        public SoundIODevice Device { get; private set; }

        public List<SoundIODevice> Devices { get; } = new List<SoundIODevice>();

        public SoundIODevice DefaultDevice { get; }
        
        public AudioFormat Format { get; private set; }
        
        public TimeSpan DesiredLatency { get; set; } = TimeSpan.FromMilliseconds(10);
        
        public TimeSpan ActualLatency { get; private set; }
        
        private void Write(float[] buffer)
        {
            ringBuffer.Enqueue(buffer);
        }
        
        public void Initialize(SoundIODevice device, AudioFormat format)
        {
            if (format.Channels != 1)
            {
                throw new OutputInitializationException("Format must qualify channels == 1");
            }
            
            Device = device;
            Format = format;

            var bytesPerSample = format.BitDepth / 8;
            var capacity = Format.SampleRate * Format.Channels * bytesPerSample * 30;
            ringBuffer = new RingBuffer<float>((uint)capacity);
            if (Device.ProbeError != 0)
            {
                throw new OutputInitializationException($"Probe Error : {Device.ProbeError}");
            }

            outstream = Device.CreateOutStream();
            outstream.WriteCallback = (min, max) => write_callback(outstream, min, max);
            outstream.UnderflowCallback = () => underflow_callback(outstream);
            outstream.SampleRate = Format.SampleRate;
            outstream.SoftwareLatency = DesiredLatency.TotalSeconds;
            outstream.Format = SoundIODevice.S16NE;

            outstream.Open();
            outstream.Layout = SoundIOChannelLayout.GetDefault(Format.Channels);
            outstream.SoftwareLatency = DesiredLatency.TotalSeconds;
            api.FlushEvents();

            ActualLatency = TimeSpan.FromSeconds(outstream.SoftwareLatency);

        }

        public void Play()
        {
            IsRunning = true;
            outstream.Start();
        }

        public void Stop()
        {
            IsRunning = false;
            outstream?.Dispose();
            outstream = null;
        }

        public void Dispose()
        {
            Stop();

            foreach (var device in Devices)
            {
                device.RemoveReference();
            }

            api.Disconnect();
            api.Dispose();
        }
        
        void write_callback(SoundIOOutStream outstream, int frame_count_min, int frame_count_max)
        {
            int frameCount = frame_count_max;
            if (frameCount <= 0)
            {
                return;
            }
            
            var results = outstream.BeginWrite(ref frameCount);
            var samples = new float[frameCount];
            
            var e = new OutputBufferEventArgs(frameCount);
            Buffer?.Invoke(this, e);
            if (e.Buffer != null)
            {
                ringBuffer.Enqueue(e.Buffer);
            }
            
            ringBuffer.Dequeue(samples, frameCount);

            var layout = outstream.Layout;
            for (int frame = 0; frame < frameCount; frame++)
            {
                for (int channel = 0; channel < layout.ChannelCount; channel++)
                {
                    var area = results.GetArea(channel);
                    // Raspberry Piではなぜかshortじゃないと音がプツプツする
                    write_short_sample(area.Pointer, samples[frame]);
                    area.Pointer += area.Step;
                }
            }

            outstream.EndWrite();

            unsafe void write_short_sample(IntPtr ptr, float sample)
            {
                short* buf = (short*)ptr;
                *buf = (short)(sample * short.MaxValue);
            }

            unsafe void write_float_sample(IntPtr ptr, float sample)
            {
                float* buf = (float*)ptr;
                *buf = sample;
            }
        }

        void underflow_callback(SoundIOOutStream outstream)
        {
            Underflow?.Invoke(this, EventArgs.Empty);
        }
    }
}
