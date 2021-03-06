﻿using KSynthesizer;
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

namespace Synthesizer.Windows
{
    /// <summary>
    /// WaveView.xaml の相互作用ロジック
    /// </summary>
    public partial class WaveView : UserControl
    {
        public BufferRecorder PlotSource { get; set; }

        private OxyPlot.PlotModel WavePlotModel { get; } = new OxyPlot.PlotModel();

        private OxyPlot.Series.LineSeries WaveSeries { get; } = new OxyPlot.Series.LineSeries();

        private OxyPlot.Axes.LinearAxis WaveXAxis { get; } = new OxyPlot.Axes.LinearAxis() { Minimum = 0, Maximum = 1, Position = OxyPlot.Axes.AxisPosition.Bottom };

        private OxyPlot.Axes.LinearAxis WaveYAxis { get; } = new OxyPlot.Axes.LinearAxis() { Minimum = -1.1, Maximum = 1.1, Position = OxyPlot.Axes.AxisPosition.Left };

        private DispatcherTimer Timer;

        public int MinimumSampleSize { get; set; } = 256;

        private void updatePlot(object sender, EventArgs e)
        {
            // Read buffer for 1 period
            if (PlotSource == null)
            {
                WaveSeries.Points.Clear();
                WavePlotModel.InvalidatePlot(true);
                return;
            }

            int sample = MinimumSampleSize;
            float[] buffer;

            PlotSource.BufferLength = sample;
            buffer = PlotSource.LastBuffer;
            WaveXAxis.Maximum = sample / (double)PlotSource.Format.SampleRate;
            WaveSeries.Points.Clear();

            if (buffer.Length > 0)
            {
                for (int i = 0; buffer.Length > i; i++)
                {
                    double time = i / (double)(PlotSource.Format.SampleRate - 1);
                    WaveSeries.Points.Add(new OxyPlot.DataPoint(time, buffer[i]));
                }
            }

            WavePlotModel.InvalidatePlot(true);
        }

        public WaveView()
        {
            InitializeComponent();
            Timer = new DispatcherTimer(TimeSpan.FromSeconds(0.1), DispatcherPriority.Loaded, new EventHandler(updatePlot), Dispatcher);
            Timer.Stop();

            WavePlotModel.Axes.Add(WaveXAxis);
            WavePlotModel.Axes.Add(WaveYAxis);
            WavePlotModel.Series.Add(WaveSeries);
            WaveSeries.LineJoin = OxyPlot.LineJoin.Round;
            wavePlot.Model = WavePlotModel;
        }

        public void Play()
        {
            Timer.Start();
        }

        public void Pause()
        {
            Timer.Stop();
        }
    }
}
