﻿using System;
using System.Collections.Generic;
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
using System.Windows.Threading;

namespace TestTool.Windows.Views
{
    /// <summary>
    /// FftView.xaml の相互作用ロジック
    /// </summary>
    public partial class FftView : UserControl
    {
        public event EventHandler NeedBuffer;

        private OxyPlot.PlotModel WavePlotModel { get; } = new OxyPlot.PlotModel();

        private OxyPlot.Series.LineSeries WaveSeries { get; } = new OxyPlot.Series.LineSeries();

        private OxyPlot.Axes.LogarithmicAxis WaveXAxis { get; } = new OxyPlot.Axes.LogarithmicAxis() { Minimum = 0, Maximum = 1, Position = OxyPlot.Axes.AxisPosition.Bottom };

        private OxyPlot.Axes.LinearAxis WaveYAxis { get; } = new OxyPlot.Axes.LinearAxis() { Minimum = 0, Maximum = 1.1, Position = OxyPlot.Axes.AxisPosition.Left };

        private DispatcherTimer Timer;

        private void updatePlot(object sender, EventArgs e)
        {
            NeedBuffer?.Invoke(this, EventArgs.Empty);
        }

        public FftView()
        {
            InitializeComponent();
            Timer = new DispatcherTimer(TimeSpan.FromSeconds(0.5), DispatcherPriority.Loaded, new EventHandler(updatePlot), Dispatcher);
            Timer.Stop();


            WavePlotModel.Axes.Add(WaveXAxis);
            WavePlotModel.Axes.Add(WaveYAxis);
            WavePlotModel.Series.Add(WaveSeries);
            plotView.Model = WavePlotModel;
        }

        public unsafe void Process(float[] buffer, int sampleRate)
        {
            int length = 1;
            while (length <= buffer.Length)
            {
                length *= 2;
            }
            length /= 2;

            var real = new float[length];
            var img = new float[length];
            var hamming = MathNet.Numerics.Window.HammingPeriodic(length);
            fixed (float* buf = buffer)
            fixed (float* re = real)
            fixed (double* ham = hamming)
            {
                for (int i = 0; length > i; i++)
                {
                    re[i] = buf[i] * (float)ham[i];
                }
            }

            MathNet.Numerics.IntegralTransforms.Fourier.Forward(real, img, MathNet.Numerics.IntegralTransforms.FourierOptions.Default);

            double delta_freq = sampleRate / length;
            double range_freq = sampleRate / 2;
            double maxY = 0;

            WaveSeries.Points.Clear();

            fixed (float* im = img)
            fixed (float* re = real)
            {
                double abs;

                WaveSeries.Points.Add(new OxyPlot.DataPoint(1, 0));
                WaveSeries.Points.Add(new OxyPlot.DataPoint(delta_freq - 1, 0));
                for (int i = 0; length > i; i++)
                {
                    abs = Math.Sqrt(Math.Pow(re[i], 2) + Math.Pow(im[i], 2));
                    maxY = Math.Max(abs, maxY);
                    WaveSeries.Points.Add(new OxyPlot.DataPoint(delta_freq * (i + 1), abs));
                }
            }

            WaveXAxis.Maximum = range_freq;
            WaveYAxis.Maximum = maxY + 1;
            WavePlotModel.InvalidatePlot(true);
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
