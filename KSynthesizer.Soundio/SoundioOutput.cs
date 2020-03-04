using System;
using SoundIOSharp;

namespace KSynthesizer.Soundio
{
    public class SoundioOutput : IAudioOutput
    {
        private readonly SoundIO api = new SoundIO();
        private readonly SoundIODevice[] devices;
        private SoundIOOutStream outstream;

        public SoundioOutput()
        {
            api.Connect();
            api.FlushEvents();
            
            devices = new SoundIODevice[api.OutputDeviceCount];
            for (int i = 0; api.OutputDeviceCount > i; i++)
            {
                devices[i] = api.GetOutputDevice(i);
            }
        }
        
        public event EventHandler<FillBufferEventArgs> FillBuffer;

        public event EventHandler Underflow;

        public TimeSpan Latency { get; set; } = TimeSpan.FromMilliseconds(10);

        public SoundIODevice Device { get; private set; }
        
        public AudioFormat Format { get; private set; }
        
        public void SetDevice(SoundIODevice device, AudioFormat format)
        {
            Device = device;
            Format = format;
        }

        public void Play()
        {
            if (Device.ProbeError != 0)
            {
                throw new OutputInitializationException($"Probe Error : {Device.ProbeError}");
            }

            outstream = Device.CreateOutStream();
            outstream.WriteCallback = (min, max) => write_callback(outstream, min, max);
            outstream.UnderflowCallback = () => underflow_callback(outstream);
            outstream.SoftwareLatency = 0;
            outstream.SampleRate = Format.SampleRate;

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

            if (outstream.LayoutErrorMessage != null)
            {
                outstream.Dispose();
                outstream = null;
                
                throw new OutputInitializationException($"Channel Layout Error : {outstream.LayoutErrorMessage}");
            }
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
            double desiredSize = Latency.TotalSeconds * Format.SampleRate * Format.Channels;
            int frame_count = (int)Math.Max(frame_count_min, desiredSize);
            frame_count = Math.Min(frame_count, frame_count_max);
            if (frame_count == 0)
            {
                return;
            }

            var results = outstream.BeginWrite(ref frame_count);
            
            FillBufferEventArgs fillBuffer = new FillBufferEventArgs(Format, frame_count);
            FillBuffer?.Invoke(this, fillBuffer);

            SoundIOChannelLayout layout = outstream.Layout;
            for (int frame = 0; frame < frame_count; frame++)
            {
                for (int channel = 0; channel < layout.ChannelCount; channel++)
                {
                    var area = results.GetArea(channel);
                    write_sample(area.Pointer, fillBuffer.Buffer[frame]);
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