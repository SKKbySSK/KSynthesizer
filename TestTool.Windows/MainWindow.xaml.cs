using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using KSynthesizer.Filters;
using KSynthesizer.Sources;
using Microsoft.Win32;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using TestTool.Windows;

namespace TestTool.Windows
{
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WasapiOut _output = new WasapiOut(AudioClientShareMode.Shared, 300);
        private SynthesizerWaveProvider _provider = new SynthesizerWaveProvider();

        internal MixerFilter MainMixer { get; } = new MixerFilter();

        private LastRecordFilter<FrequencyFilter> FrequencyFilter { get; }

        private LastRecordFilter<MixerFilter> OutputSource { get; }

        public MainWindow()
        {
            InitializeComponent();

            FrequencyFilter = new LastRecordFilter<FrequencyFilter>(new FrequencyFilter(MainMixer));
            filterPanel.Filter = FrequencyFilter;

            var passSource = new MixerFilter();
            passSource.Sources.Add(FrequencyFilter);
            OutputSource = new LastRecordFilter<MixerFilter>(passSource);

            BuildMixer();
            BuildExport();
            _provider.Source = OutputSource;
            _output.Init(_provider);

            mixerView.PlotSource = OutputSource;
        }

        private void BuildMixer()
        {
            MainMixer.Sources.Add(oscPanel.Osc1);
            MainMixer.Sources.Add(oscPanel.Osc2);
        }

        private void BuildExport()
        {
            var sources = new IAudioSource[] { oscPanel.Osc1, oscPanel.Osc2, MainMixer, FrequencyFilter };
            for (int i = 0; sources.Length > i; i++)
            {
                IAudioSource source = sources[i];
                string text = null;
                switch (i)
                {
                    case 0:
                        text = "OSC1";
                        break;
                    case 1:
                        text = "OSC2";
                        break;
                    case 2:
                        text = "Mixer";
                        break;
                    case 3:
                        text = "FrequencyFilter";
                        break;
                }
                var item = new MenuItem()
                {
                    Header = text,
                };
                item.Click += (sender, e) =>
                {
                    WriteAudioSource(source);
                };

                exportItem.Items.Add(item);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Pause();
            _output.Dispose();
        }

        private void volumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _provider.Volume = (float)e.NewValue;
        }

        private void toggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (_output.PlaybackState == PlaybackState.Playing)
            {
                Pause();
            }
            else
            {
                Play();
            }
        }

        public void Pause()
        {
            _output.Pause();
            oscPanel.Pause();
            mixerView.Pause();
            fftView.Pause();
            toggleButton.Content = "Play";
        }

        public void Play()
        {
            _output.Play();
            oscPanel.Play();
            mixerView.Play();
            fftView.Play();
            toggleButton.Content = "Pause";
        }

        private void WriteAudioSource(IAudioSource source)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "WAVE File|*.wav";
            var now = DateTime.Now;
            sfd.FileName = $"{now.Month}-{now.Day} {now.Hour}-{now.Minute}-{now.Second}.wav";
            if (sfd.ShowDialog() ?? false)
            {
                var path = sfd.FileName;
                var window = new DurationDialog();
                window.Saved += Window_Saved;

                async void Window_Saved(object sender, EventArgs e)
                {
                    Pause();
                    progressOverlay.Visibility = Visibility.Visible;
                    window.Saved -= Window_Saved;
                    using (var file = new FileStream(path, FileMode.Create))
                    {
                        await SynhesizerWaveWriter.WriteAsync(file, source, window.Duration);
                    }
                    progressOverlay.Visibility = Visibility.Hidden;
                }

                window.ShowDialog();
            }
        }

        private void fftView_NeedBuffer(object sender, EventArgs e)
        {
            if (fftCheckbox.IsChecked ?? false)
            {
                fftView.Push(OutputSource.LastBuffer);
            }
        }

        private void oscPanel_FrequencyUpdated(object sender, EventArgs e)
        {
        }

        private void oscFreq_Click(object sender, RoutedEventArgs e)
        {
            Pause();

            PeriodicSourceBase source;
            if (sender == osc1Freq)
            {
                source = oscPanel.Osc1.Source;
            } 
            else
            {
                source = oscPanel.Osc2.Source;
            }

            MainMixer.Sources.Clear();
            MainMixer.Sources.Add(source);

            FrequencyFilter output = FrequencyFilter.Source;

            var window = new FrequencyDialog();
            window.ExportClicked += Window_ExportClicked;
            window.ShowDialog();

            void Window_ExportClicked(object sender, FreqEventArgs e)
            {
                window.ExportClicked -= Window_ExportClicked;

                ExportFreqCharacteristics(e, source, output);
            }
        }

        private async void ExportFreqCharacteristics(FreqEventArgs e, PeriodicSourceBase source, FrequencyFilter output)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "CSV File|*.csv";
            var now = DateTime.Now;
            sfd.FileName = $"{now.Month}-{now.Day} {now.Hour}-{now.Minute}-{now.Second}.csv";
            if (sfd.ShowDialog() ?? false)
            {
                List<(double, double)> exportingData = new List<(double, double)>();
                progressOverlay.Visibility = Visibility.Visible;
                await Task.Run(() =>
                {
                    TimeSpan currentTime = TimeSpan.Zero;
                    int size = (int)(source.Format.SampleRate * 1);
                    for (double current = e.From; e.To >= current; current += e.Step)
                    {
                        source.SetFrequency((float)current);
                        var sourceData = source.Next(size);
                        var outputData = output.Next(size);
                        var sourceAv = sourceData.Average(f => Math.Abs(f));
                        var outputAv = outputData.Average(f => Math.Abs(f));

                        var gain = 20 * Math.Log10(outputAv / sourceAv);
                        exportingData.Add((current, gain));
                        currentTime += TimeSpan.FromSeconds(0.1);
                    }
                });

                using (var sw = new StreamWriter(sfd.FileName))
                {
                    foreach(var pair in exportingData)
                    {
                        sw.WriteLine($"{pair.Item1}, {pair.Item2}");
                    }
                }
                progressOverlay.Visibility = Visibility.Hidden;
            }
        }
    }
}