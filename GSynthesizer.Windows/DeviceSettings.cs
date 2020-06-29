using System;
using System.Collections.Generic;
using System.Text;
using KSynthesizer;
using KSynthesizer.NAudio;
using KSynthesizer.Soundio;
using KSynthesizer.Sources;
using NAudio.CoreAudioApi;
using Prism.Mvvm;
using SoundIOSharp;

namespace GSynthesizer
{
    public class DeviceSettings : BindableBase
    {
        public static AudioFormat SharedFormat { get; } = new AudioFormat(48000, 1, 32);

        public IAudioOutput Output { get; private set; }

        private bool isInitialized = false;
        public bool IsInitialized
        {
            get { return isInitialized; }
            set { SetProperty(ref isInitialized, value); }
        }

        public void Initialize(SoundIODevice device)
        {
            if (device is null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            if (IsInitialized)
            {
                throw new InvalidOperationException("Output device was already initialized");
            }

            IsInitialized = false;
            var output = new SoundioOutput();
            output.Initialize(device, SharedFormat);
            Output = output;
            IsInitialized = true;
        }

        public void Initialize(MMDevice device, AudioClientShareMode shareMode = AudioClientShareMode.Shared, TimeSpan latency = default)
        {
            if (device is null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            if (IsInitialized)
            {
                throw new InvalidOperationException("Output device was already initialized");
            }

            if (latency <= TimeSpan.Zero)
            {
                latency = TimeSpan.FromMilliseconds(50);
            }

            IsInitialized = false;
            var output = new WasapiOutput();
            output.Initialize(device, shareMode, latency, SharedFormat);
            output.Play();
            Output = output;
            IsInitialized = true;
        }

        public void DisposeOutput()
        {
            IsInitialized = false;
            Output?.Dispose();
            Output = null;
        }
    }
}
