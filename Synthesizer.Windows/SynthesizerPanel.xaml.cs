using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using KSynthesizer;
using KSynthesizer.Midi;
using KSynthesizer.Soundio;

namespace Synthesizer.Windows
{
    class InterceptableSource : IAudioSource
    {
        Action<float[]> interceptor;

        public IAudioSource Source { get; set; }

        public AudioFormat Format => Source.Format;

        public void Intercept(Action<float[]> interceptor)
        {
            this.interceptor = interceptor;
        }

        public float[] Next(int size)
        {
            var buffer = Source.Next(size);
            interceptor?.Invoke(buffer);
            return buffer;
        }
    }

    /// <summary>
    /// SynthesizerPanel.xaml の相互作用ロジック
    /// </summary>
    public partial class SynthesizerPanel : UserControl
    {
        private bool isCustomMode = false;

        private KSynthesizer.Synthesizer Synthesizer { get; set; }

        private MidiPlayer CustomMidiSource { get; set; }

        private InterceptableSource Interceptable { get; } = new InterceptableSource();

        public bool IsCustomMode
        {
            get => isCustomMode;
            private set
            {
                isCustomMode = value;
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    keyboard.IsEnabled = !value;
                }));
            }
        }

        private SoundioOutput Output { get; } = new SoundioOutput();

        private Dictionary<IonianTone, int> OscIndicies { get; } = new Dictionary<IonianTone, int>();

        private Dictionary<Key, bool> KeyStates { get; } = new Dictionary<Key, bool>();

        private float Volume { get; set; }

        public int PhysicalKeyOctave { get; set; } = 3;

        public BufferRecorder Recorder { get; }

        public AudioFormat Format { get; } = new AudioFormat(44100, 1, 32);

        public SynthesizerPanel()
        {
            InitializeComponent();

            var index = 0;
            var defaultDevice = 0;
            foreach (var device in Output.Devices)
            {
                devicesBox.Items.Add(device.Name);
                if (device.Id == Output.DefaultDevice?.Id)
                {
                    defaultDevice = index;
                }
                index++;
            }
            devicesBox.SelectedIndex = defaultDevice;

            Recorder = new BufferRecorder(Format);
            waveView.PlotSource = Recorder;

            Synthesizer = new KSynthesizer.Synthesizer(Format.SampleRate, 8);
            Interceptable.Source = Synthesizer;
            Interceptable.Intercept(Interceptor);
            UpdateSynthesizer();
        }

        public void SetCustomMidiSource(MidiPlayer source)
        {
            if (source == null)
            {
                if (Interceptable.Source is MidiPlayer midi)
                {
                    midi.Finished -= Source_Finished;
                    midi.EventReceived -= Source_EventReceived;
                }

                Interceptable.Source = Synthesizer;
                IsCustomMode = false;
                UpdateSynthesizer();
                return;
            }

            source.EventReceived += Source_EventReceived;
            source.OscillatorConfigs.Clear();
            source.OscillatorConfigs.AddRange(GenerateMidiConfig());
            source.Finished += Source_Finished;
            CustomMidiSource = source;
            Interceptable.Source = source;
            IsCustomMode = true;
            UpdateSynthesizer();
        }

        private void Source_EventReceived(object sender, MidiEventArgs e)
        {
            if (e.Processed)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[MIDI] Processed : {e.Event}");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[MIDI] Ignored : {e.Event}");
            }
            Console.ForegroundColor = ConsoleColor.White;
        }

        private void Source_Finished(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                SetCustomMidiSource(null);
            }));
        }

        private void prepareButton_Click(object sender, RoutedEventArgs e)
        {
            if (devicesBox.SelectedIndex >= 0)
            {
                try
                {
                    Output.SetDevice(Output.Devices[devicesBox.SelectedIndex], Format);
                    Output.FillBuffer += Output_FillBuffer;
                    Volume = (float)volumeSlider.Value;

                    Output.Play();
                    waveView.Play();
                    devicesBox.IsEnabled = false;
                    prepareButton.Visibility = Visibility.Hidden;
                    stopButton.Visibility = Visibility.Visible;
                }
                catch (Exception ex)
                {
                    waveView.Pause();
                    MessageBox.Show("内部エラーが発生しました。\n別のデバイスで試してみてください\n\n" + ex.ToString());
                }
            }
        }

        private void Output_FillBuffer(object sender, FillBufferEventArgs e)
        {
            int channelLen = e.Size / e.Format.Channels;
            var channelBuffer = Interceptable.Next(channelLen);
            var buffer = new float[e.Size];

            for (int i = 0; e.Format.Channels > i; i++)
            {
                Array.Copy(channelBuffer, 0, buffer, i * channelLen, channelLen);
            }

            e.Configure(buffer);
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            waveView.Pause();
            devicesBox.IsEnabled = true;
            stopButton.Visibility = Visibility.Hidden;
            prepareButton.Visibility = Visibility.Visible;
        }

        public void Dispose()
        {
            SetCustomMidiSource(null);
            waveView.Pause();
            Output.Dispose();
        }

        private void KeyboardView_KeyDown(object sender, ValueEventArgs<IonianTone> e)
        {
            if (Synthesizer == null)
            {
                return;
            }

            var index = Synthesizer.Attack(GenerateConfig(e.Value));
            OscIndicies[e.Value] = index;
        }

        private void KeyboardView_KeyUp(object sender, ValueEventArgs<IonianTone> e)
        {
            if (OscIndicies.TryGetValue(e.Value, out var index))
            {
                Synthesizer?.Release(index);
            }
        }

        public void OnPhysicalKeyDown(Key key)
        {
            var tone = GetToneForKey(key, PhysicalKeyOctave);
            if (tone == null) return;
            keyboard.SimulateKeyDown(tone.Value);
        }

        public void OnPhysicalKeyUp(Key key)
        {
            var tone = GetToneForKey(key, PhysicalKeyOctave);
            if (tone == null) return;
            keyboard.SimulateKeyUp(tone.Value);
        }

        private List<OscillatorConfig> GenerateConfig(IonianTone tone)
        {
            var tone1 = tone;
            tone1.Semitones = (int)Math.Round(semitone1.Value);
            var tone2 = tone;
            tone2.Semitones = (int)Math.Round(semitone2.Value);
            return GenerateConfig(Scale.GetFrequency(tone1), Scale.GetFrequency(tone2));
        }

        private List<OscillatorConfig> GenerateMidiConfig()
        {
            return new List<OscillatorConfig>(new[]
            {
                new OscillatorConfig(){ Function = Osc1.Function },
            });
        }

        private List<OscillatorConfig> GenerateConfig(float freq1, float freq2)
        {
            return Helper.ConfigGenerator.GenerateConfig(new []
            {
                (Osc1.Function, freq1),
                (Osc2.Function, freq2),
            }, (filter) =>
            {
                filter.Beta = (float)((modBeta.Value / 100) * 15);
            });
        }

        private IonianTone? GetToneForKey(Key key, int octave)
        {
            IonianScale scale;
            switch (key)
            {
                case Key.D1:
                    scale = IonianScale.A;
                    break;
                case Key.D2:
                    scale = IonianScale.B;
                    break;
                case Key.D3:
                    scale = IonianScale.C;
                    break;
                case Key.D4:
                    scale = IonianScale.D;
                    break;
                case Key.D5:
                    scale = IonianScale.E;
                    break;
                case Key.D6:
                    scale = IonianScale.F;
                    break;
                case Key.D7:
                    scale = IonianScale.G;
                    break;
                case Key.D8:
                    octave++;
                    scale = IonianScale.A;
                    break;
                case Key.D9:
                    octave++;
                    scale = IonianScale.B;
                    break;
                case Key.D0:
                    octave++;
                    scale = IonianScale.C;
                    break;
                default:
                    return null;
            }

            return new IonianTone()
            {
                Scale = scale,
                Sharp = false,
                Octave = octave
            };
        }

        private void Interceptor(float[] buffer)
        {
            Recorder.Append(buffer);
            for (int i = 0; buffer.Length > i; i++)
            {
                buffer[i] *= Volume;
            }
        }

        private void volumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Volume = (float)volumeSlider.Value;
        }

        private void Osc_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CustomMidiSource != null)
            {
                CustomMidiSource.OscillatorConfigs[0].Function = Osc1.Function;
            }
        }

        private void UpdateSynthesizer()
        {
            var synthesizer = IsCustomMode ? CustomMidiSource.Synthesizer : Synthesizer;

            synthesizer.AttackDuration = TimeSpan.FromMilliseconds(10);
            synthesizer.DecayDuration = TimeSpan.FromMilliseconds(4000);
            synthesizer.Sustain = 0;
            synthesizer.ReleaseDuration = TimeSpan.FromMilliseconds(4000);
            freqPanel.Filter = synthesizer.FrequencyFilter;
        }
    }
}
