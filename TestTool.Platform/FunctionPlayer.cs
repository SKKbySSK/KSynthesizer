﻿using System;
using System.Collections.Generic;
using System.Text;
using SoundIOSharp;
using KSynthesizer.Sources;
using System.Diagnostics;

namespace TestTool.Platform
{
    class FunctionPlayer
    {
        SoundIO api = new SoundIO();
		SoundIOOutStream outstream;

        public FunctionPlayer()
        {
            api.Connect();
            api.FlushEvents();

            OutputDevice = api.GetOutputDevice(api.DefaultOutputDeviceIndex);

            if (OutputDevice.ProbeError != 0)
            {
                throw new Exception($"Probe Error : {OutputDevice.ProbeError}");
            }

			outstream = OutputDevice.CreateOutStream();
			outstream.WriteCallback = (min, max) => write_callback(outstream, min, max);
			outstream.UnderflowCallback = () => underflow_callback(outstream);
			outstream.SoftwareLatency = 0;
			outstream.SampleRate = Source.Format.SampleRate;

			if (OutputDevice.SupportsFormat(SoundIODevice.Float32NE))
			{
				outstream.Format = SoundIODevice.Float32NE;
			}
			else
			{
                throw new Exception("No suitable format");
			}

			outstream.Open();
		}

        public SoundIOBackend Backend => api.CurrentBackend;

        public SoundIODevice OutputDevice { get; }

		public PeriodicFunctionsSource Source { get; } = new PeriodicFunctionsSource(44100);

		public double WriteLatency { get; set; } = 0.05f;

		public void Start()
		{
			outstream.Start();
			api.FlushEvents();
		}

		public void Dispose()
		{
			outstream.Dispose();
			OutputDevice.RemoveReference();
			api.Dispose();
		}

		void write_callback(SoundIOOutStream outstream, int frame_count_min, int frame_count_max)
		{
			int frames_left = frame_count_min;
			var writeSize = (int)(Source.Format.SampleRate * WriteLatency);

			int frame_count = Math.Max(frames_left, Math.Min(writeSize, frame_count_max));
			if (frame_count == 0)
			{
				return;
			}

			var results = outstream.BeginWrite(ref frame_count);
			SoundIOChannelLayout layout = outstream.Layout;

			var buffer = Source.Next(frame_count);
			for (int frame = 0; frame < frame_count; frame++)
			{
				for (int channel = 0; channel < layout.ChannelCount; channel++)
				{
					var area = results.GetArea(channel);
					write_sample(area.Pointer, buffer[frame]);
					area.Pointer += area.Step;
				}
			}

			outstream.EndWrite();
		}

		static unsafe void write_sample(IntPtr ptr, float sample)
		{
			float* buf = (float*)ptr;
			*buf = sample;
		}

		static int underflow_callback_count = 0;
		static void underflow_callback(SoundIOOutStream outstream)
		{
			Console.Error.WriteLine("underflow {0}", underflow_callback_count++);
		}
	}
}
