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
        WasapiOut _output = new WasapiOut(AudioClientShareMode.Shared, 300);
        SynthesizerWaveProvider _provider = new SynthesizerWaveProvider();
        private MixerFilter mixer = new MixerFilter();
        private MixerFilter mixerPlot = new MixerFilter();
        
        public MainWindow()
        {
            InitializeComponent();
            mixer.Sources.Add(osc1.Source);
            mixer.Sources.Add(osc2.Source);
            mixerPlot.Sources.Add(osc1.PlotSource);
            mixerPlot.Sources.Add(osc2.PlotSource);
            _provider.Source = mixer;
            _output.Init(_provider);
            _output.Play();

            mixerView.PlotSource = mixerPlot;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _output.Dispose();
        }
    }
}