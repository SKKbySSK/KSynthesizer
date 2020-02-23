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

        internal CustomMixerFilter MainMixer { get; } = new CustomMixerFilter();

        public MainWindow()
        {
            InitializeComponent();

            BuildMixer();
            _provider.Source = MainMixer;
            _output.Init(_provider);

            mixerView.PlotSource = MainMixer;
        }

        private void BuildMixer()
        {
            MainMixer.Sources.Add(oscPanel.Osc1);
            MainMixer.Sources.Add(oscPanel.Osc2);
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
            toggleButton.Content = "Play";
        }

        public void Play()
        {
            _output.Play();
            oscPanel.Play();
            mixerView.Play();
            toggleButton.Content = "Pause";
        }

        private void exportOsc1_Click(object sender, RoutedEventArgs e)
        {
            WriteAudioSource(oscPanel.Osc1);
        }

        private void exportOsc2_Click(object sender, RoutedEventArgs e)
        {
            WriteAudioSource(oscPanel.Osc2);
        }

        private void exportMixer_Click(object sender, RoutedEventArgs e)
        {
            WriteAudioSource(MainMixer);
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
    }
}