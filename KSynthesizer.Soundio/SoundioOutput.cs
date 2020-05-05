﻿using System;
using System.Collections.Generic;
using SoundIOSharp;

namespace KSynthesizer.Soundio
{
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
                if (device.SupportsFormat(SoundIODevice.Float32NE))
                {
                    if (i == api.DefaultOutputDeviceIndex)
                    {
                        DefaultDevice = device;
                    }

                    Devices.Add(device);
                }
            }
        }

        public event EventHandler Underflow;

        public event EventHandler<int> WillFilled;

        public SoundIODevice Device { get; private set; }

        public List<SoundIODevice> Devices { get; } = new List<SoundIODevice>();

        public SoundIODevice DefaultDevice { get; }
        
        public AudioFormat Format { get; private set; }
        
        public TimeSpan DesiredLatency { get; set; } = TimeSpan.FromMilliseconds(10);
        
        public TimeSpan ActualLatency { get; private set; }

        public void Write(float[] buffer)
        {
            ringBuffer.Enqueue(buffer);
        }
        
        public void Initialize(SoundIODevice device, AudioFormat format)
        {
            if (format.Channels != 1 || format.BitDepth != 32)
            {
                throw new OutputInitializationException("Format must qualify channels == 1 && bitDepth == 32");
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

            if (Device.SupportsFormat(SoundIODevice.Float32NE))
            {
                outstream.Format = SoundIODevice.Float32NE;
            }
            else
            {
                outstream.Dispose();
                outstream = null;
                
                throw new OutputInitializationException($"No suitable format");
            }
            
            outstream.Open();
            outstream.SoftwareLatency = DesiredLatency.TotalSeconds;
            api.FlushEvents();

            ActualLatency = TimeSpan.FromSeconds(outstream.SoftwareLatency);

        }

        public void Play()
        {
            outstream.Start();
        }

        public void Stop()
        {
            outstream?.Dispose();
            outstream = null;
        }

        public void Dispose()
        {
            Stop();
            api.Dispose();
        }
        
        void write_callback(SoundIOOutStream outstream, int frame_count_min, int frame_count_max)
        {            
            double desiredSize = DesiredLatency.TotalSeconds * Format.SampleRate * Format.Channels;
            int frame_count = (int)Math.Max(frame_count_min, desiredSize);
            frame_count = Math.Min(frame_count, frame_count_max);
            if (frame_count == 0)
            {
                return;
            }

            var results = outstream.BeginWrite(ref frame_count);
            WillFilled?.Invoke(this, frame_count);
            var samples = new float[frame_count];
            ringBuffer.Dequeue(samples);

            SoundIOChannelLayout layout = outstream.Layout;
            for (int frame = 0; frame < frame_count; frame++)
            {
                for (int channel = 0; channel < layout.ChannelCount; channel++)
                {
                    var area = results.GetArea(channel);
                    write_sample(area.Pointer, samples[frame]);
                    area.Pointer += area.Step;
                }
            }

            outstream.EndWrite();

            unsafe void write_sample(IntPtr ptr, float sample)
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