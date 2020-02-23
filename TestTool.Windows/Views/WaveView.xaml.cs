using KSynthesizer;
using KSynthesizer.Sources;
using System;
using System.Collections.Generic;
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
using System.Windows.Threading;

namespace TestTool.Windows.Views
{
    /// <summary>
    /// WaveView.xaml の相互作用ロジック
    /// </summary>
    public partial class WaveView : UserControl
    {
        public IAudioSource PlotSource { get; set; }

        private OxyPlot.PlotModel WavePlotModel { get; } = new OxyPlot.PlotModel();

        private OxyPlot.Series.LineSeries WaveSeries { get; } = new OxyPlot.Series.LineSeries();

        private OxyPlot.Axes.LinearAxis WaveXAxis { get; } = new OxyPlot.Axes.LinearAxis() { Minimum = 0, Maximum = 1, Position = OxyPlot.Axes.AxisPosition.Bottom };

        private OxyPlot.Axes.LinearAxis WaveYAxis { get; } = new OxyPlot.Axes.LinearAxis() { Minimum = -1.1, Maximum = 1.1, Position = OxyPlot.Axes.AxisPosition.Left };

        internal void StartPlotTimer()
        {
            var timer = new DispatcherTimer(TimeSpan.FromSeconds(0.1), DispatcherPriority.Loaded, new EventHandler(updatePlot), Dispatcher);
            timer.Start();
        }

        private void updatePlot(object sender, EventArgs e)
        {
            // Read buffer for 1 period
            if (PlotSource == null)
            {
                WaveSeries.Points.Clear();
                WavePlotModel.InvalidatePlot(true);
                return;
            }

            int sample = 200;
            if (PlotSource is PeriodicSourceBase periodic)
            {
                sample = Math.Max((int)(PlotSource.Format.SampleRate * periodic.Period / 1000), 200);
            }

            WaveXAxis.Maximum = sample / (double)PlotSource.Format.SampleRate;
            var buffer = PlotSource.Next(sample);
            WaveSeries.Points.Clear();

            for (int i = 0; buffer.Length > i; i++)
            {
                double time = i / (double)(PlotSource.Format.SampleRate - 1);
                WaveSeries.Points.Add(new OxyPlot.DataPoint(time, buffer[i]));
            }

            WavePlotModel.InvalidatePlot(true);
        }

        public WaveView()
        {
            InitializeComponent();
            WavePlotModel.Axes.Add(WaveXAxis);
            WavePlotModel.Axes.Add(WaveYAxis);
            WavePlotModel.Series.Add(WaveSeries);
            WaveSeries.LineJoin = OxyPlot.LineJoin.Round;
            wavePlot.Model = WavePlotModel;
            StartPlotTimer();
        }
    }
}
