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
using KSynthesizer.Soundio;

namespace Synthesizer.Windows
{
    /// <summary>
    /// SynthesizerPanel.xaml の相互作用ロジック
    /// </summary>
    public partial class SynthesizerPanel : UserControl
    {
        private KSynthesizer.Synthesizer Synthesizer { get; set; }

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
        }

        private void prepareButton_Click(object sender, RoutedEventArgs e)
        {
            if (devicesBox.SelectedIndex >= 0)
            {
                try
                {
                    Output.SetDevice(Output.Devices[devicesBox.SelectedIndex], Format);
                    Synthesizer = new KSynthesizer.Synthesizer(Output, 8);
                    Synthesizer.Intercept(Interceptor);
                    Volume = (float)volumeSlider.Value;

                    Output.Play();
                    waveView.Play();
                    devicesBox.IsEnabled = false;
                    prepareButton.Visibility = Visibility.Hidden;
                    stopButton.Visibility = Visibility.Visible;
                }
                catch(Exception ex)
                {
                    waveView.Pause();
                    Synthesizer?.Dispose();
                    Synthesizer = null;
                    MessageBox.Show("内部エラーが発生しました。\n別のデバイスで試してみてください\n\n" + ex.ToString());
                }
            }
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            waveView.Pause();
            Synthesizer?.Dispose();
            Synthesizer = null;

            devicesBox.IsEnabled = true;
            stopButton.Visibility = Visibility.Hidden;
            prepareButton.Visibility = Visibility.Visible;
        }

        public void Dispose()
        {
            waveView.Pause();
            Output.Dispose();
            Synthesizer?.Dispose();
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
            if (Synthesizer == null)
            {
                return;
            }

            if (KeyStates.TryGetValue(key, out var pressed) && pressed)
            {
                return;
            }

            var tone = GetToneForKey(key, PhysicalKeyOctave);
            if (tone == null) return;

            var index = Synthesizer.Attack(GenerateConfig(tone.Value));
            OscIndicies[tone.Value] = index;
            KeyStates[key] = true;
        }

        private List<OscillatorConfiguration> GenerateConfig(IonianTone tone)
        {
            var tone1 = tone;
            tone1.Semitones = (int)Math.Round(semitone1.Value);
            var tone2 = tone;
            tone2.Semitones = (int)Math.Round(semitone2.Value);

            var configurations = new List<OscillatorConfiguration>();
            configurations.Add(new OscillatorConfiguration()
            {
                Function = Osc1.Function,
                Frequency = Scale.GetFrequency(tone1),
            });

            configurations.Add(new OscillatorConfiguration()
            {
                Function = Osc2.Function,
                Frequency = Scale.GetFrequency(tone2),
            });
            return configurations;
        }

        public void OnPhysicalKeyUp(Key key)
        {
            var tone = GetToneForKey(key, PhysicalKeyOctave);
            if (tone == null) return;
            KeyStates[key] = false;
            if (OscIndicies.TryGetValue(tone.Value, out var index))
            {
                Synthesizer?.Release(index);
            }
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
    }
}
