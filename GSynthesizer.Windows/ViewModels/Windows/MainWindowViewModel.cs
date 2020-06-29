using GSynthesizer.Windows;
using GSynthesizer.Windows.Views;
using KSynthesizer;
using KSynthesizer.Filters;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace GSynthesizer.ViewModels.Windows
{
    public class MainWindowViewModel : BindableBase, IDisposable
    {
        private readonly DeviceSettings deviceSettings;
        private readonly MainSynthesizer mainSynthesizer;
        private readonly Dictionary<IonianTone, int> ids = new Dictionary<IonianTone, int>();
        private readonly RingBuffer<float> waveViewBuffer = new RingBuffer<float>((uint)(DeviceSettings.SharedFormat.SampleRate * 0.2));
        private readonly int WaveBufferLength = (int)(DeviceSettings.SharedFormat.SampleRate * 0.05);

        public MainWindowViewModel(DeviceSettings deviceSettings, MainSynthesizer synthesizer)
        {
            this.deviceSettings = deviceSettings;
            mainSynthesizer = synthesizer;
            synthesizer.Intercepted += Synthesizer_Intercepted;

            mainSynthesizer.Synthesizer.AttackDuration = TimeSpan.FromMilliseconds(AttackMs);
            mainSynthesizer.Synthesizer.DecayDuration = TimeSpan.FromMilliseconds(DecayMs);
            mainSynthesizer.Synthesizer.Sustain = (float)SustainLevel;
            mainSynthesizer.Synthesizer.ReleaseDuration = TimeSpan.FromMilliseconds(ReleaseMs);
        }

        private void Synthesizer_Intercepted(object sender, InterceptedEventArgs e)
        {
            var len = waveViewBuffer.GetLength();
            if (len >= WaveBufferLength)
            {
                var buffer = SynthesizerBuffer ?? new AudioBuffer() { Floats = new float[WaveBufferLength], SampleRate = e.Format.SampleRate };
                waveViewBuffer.Dequeue(buffer.Floats, WaveBufferLength);

                try
                {
                    Dispatcher.CurrentDispatcher.Invoke(() =>
                    {
                        if (SynthesizerBuffer == null)
                        {
                            SynthesizerBuffer = buffer;
                        }
                        else
                        {
                            SynthesizerBuffer = new AudioBuffer()
                            {
                                Floats = buffer.Floats,
                                SampleRate = e.Format.SampleRate
                            };
                        }
                    });
                }
                catch(TaskCanceledException)
                {

                }
            }

            waveViewBuffer.Enqueue(e.Buffer);
        }

        private AudioBuffer synthesizerBuffer;
        public AudioBuffer SynthesizerBuffer
        {
            get { return synthesizerBuffer; }
            private set { SetProperty(ref synthesizerBuffer, value); }
        }

        private DelegateCommand<IonianTone?> _onAttack;
        public DelegateCommand<IonianTone?> OnAttack =>
            _onAttack ?? (_onAttack = new DelegateCommand<IonianTone?>(ExecuteOnAttack));

        void ExecuteOnAttack(IonianTone? tone)
        {
            if (tone == null)
            {
                return;
            }

            var id = mainSynthesizer.Synthesizer.Attack(new[]
            {
                new Oscillator()
                {
                    Frequency = Scale.GetFrequency(tone.Value),
                }
            });

            ids[tone.Value] = id;
        }

        private DelegateCommand<IonianTone?> _onRelease;
        public DelegateCommand<IonianTone?> OnRelease =>
            _onRelease ?? (_onRelease = new DelegateCommand<IonianTone?>(ExecuteOnRelease));

        private DelegateCommand<string> _openMidi;
        public DelegateCommand<string> OpenMidi =>
            _openMidi ?? (_openMidi = new DelegateCommand<string>(ExecuteOpenMidi));

        void ExecuteOpenMidi(string path)
        {
            mainSynthesizer.OpenMidi(path);
        }

        void ExecuteOnRelease(IonianTone? tone)
        {
            if (tone == null)
            {
                return;
            }

            if (ids.TryGetValue(tone.Value, out var id))
            {
                mainSynthesizer.Synthesizer.Release(id);
            }
        }

        private FrequencyFilterMode filterMode;
        public FrequencyFilterMode FilterMode
        {
            get { return filterMode; }
            set
            {
                SetProperty(ref filterMode, value);
                UpdateFilter(value);
            }
        }

        private void UpdateFilter(FrequencyFilterMode value)
        {
            switch (value)
            {
                case FrequencyFilterMode.Disable:
                    mainSynthesizer.Synthesizer.FrequencyFilter.Disable();
                    break;
                case FrequencyFilterMode.Lowpass:
                    mainSynthesizer.Synthesizer.FrequencyFilter.SetLowpassMode((float)LowFrequency);
                    break;
                case FrequencyFilterMode.Highpass:
                    mainSynthesizer.Synthesizer.FrequencyFilter.SetHighpassMode((float)HighFrequency);
                    break;
                case FrequencyFilterMode.Bandpass:
                    if (HighFrequency < LowFrequency)
                    {
                        HighFrequency = LowFrequency;
                    }
                    mainSynthesizer.Synthesizer.FrequencyFilter.SetBandpassMode((float)LowFrequency, (float)HighFrequency);
                    break;
            }
        }

        private double lowFrequency = 300;
        public double LowFrequency
        {
            get { return lowFrequency; }
            set
            {
                SetProperty(ref lowFrequency, value);
                if (FilterMode == FrequencyFilterMode.Bandpass && value + 50 > HighFrequency)
                {
                    HighFrequency = value + 50;
                }
                else
                {
                    UpdateFilter(FilterMode);
                }
            }
        }

        private double highFrequency = 1500;
        public double HighFrequency
        {
            get { return highFrequency; }
            set
            {
                SetProperty(ref highFrequency, value);
                if (FilterMode == FrequencyFilterMode.Bandpass && value - 50 < LowFrequency)
                {
                    LowFrequency = value - 50;
                }
                else
                {
                    UpdateFilter(FilterMode);
                }
            }
        }

        private double attackMs = 10;
        public double AttackMs
        {
            get { return attackMs; }
            set
            {
                SetProperty(ref attackMs, value);
                mainSynthesizer.Synthesizer.AttackDuration = TimeSpan.FromMilliseconds(value);
            }
        }

        private double decayMs = 500;
        public double DecayMs
        {
            get { return decayMs; }
            set
            {
                SetProperty(ref decayMs, value);
                mainSynthesizer.Synthesizer.DecayDuration = TimeSpan.FromMilliseconds(value);
            }
        }

        private double sustainLevel = 0;
        public double SustainLevel
        {
            get { return sustainLevel; }
            set
            {
                SetProperty(ref sustainLevel, value);
                mainSynthesizer.Synthesizer.Sustain = (float)value;
            }
        }

        private double releaseMs = 20;
        public double ReleaseMs
        {
            get { return releaseMs; }
            set
            {
                SetProperty(ref releaseMs, value);
                mainSynthesizer.Synthesizer.ReleaseDuration = TimeSpan.FromMilliseconds(value);
            }
        }

        public bool StartRecording()
        {
            return mainSynthesizer.StartRecording();
        }

        public Task StopRecording(string outputPath)
        {
            return mainSynthesizer.StopRecording(outputPath);
        }

        public void Dispose()
        {
            mainSynthesizer.Intercepted -= Synthesizer_Intercepted;
            mainSynthesizer.Stop();
            deviceSettings.DisposeOutput();
        }
    }
}
