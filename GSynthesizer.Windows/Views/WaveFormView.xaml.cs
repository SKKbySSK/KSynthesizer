using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GSynthesizer.Windows.Views
{
    public class AudioBuffer
    {
        public int SampleRate { get; set; }

        public float[] Floats { get; set; }
    }

    /// <summary>
    /// Interaction logic for WaveFormView
    /// </summary>
    public partial class WaveFormView : UserControl
    {
        public WaveFormView()
        {
            InitializeComponent();
            model.Axes.Add(xAxis);
            model.Axes.Add(yAxis);
            model.Series.Add(lineSeries);
            plotView.Model = model;

            UpdateMinimumAndMaximum();
            UpdatePlot();
        }

        private TimeSpanAxis xAxis = new TimeSpanAxis()
        {
            Position = AxisPosition.Bottom,
            LabelFormatter = (val) =>
            {
                return TimeSpanAxis.ToTimeSpan(val).ToString("ss'.'fff");
            },
            MajorStep = TimeSpanAxis.ToDouble(TimeSpan.FromSeconds(0.5)),
            MajorGridlineStyle = LineStyle.Solid,
            Minimum = TimeSpanAxis.ToDouble(TimeSpan.FromSeconds(0)),
            Maximum = TimeSpanAxis.ToDouble(TimeSpan.FromSeconds(1)),
        };

        private LinearAxis yAxis = new LinearAxis()
        {
            Position = AxisPosition.Left,
        };

        private LineSeries lineSeries = new LineSeries()
        {
            StrokeThickness = 2,
            Color = OxyColors.Red,
        };

        private PlotModel model = new PlotModel() { };

        public double? Minimum
        {
            get { return (double?)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(double?), typeof(WaveFormView), new PropertyMetadata(new double?(-1)));

        public double? Maximum
        {
            get { return (double?)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(double?), typeof(WaveFormView), new PropertyMetadata(new double?(1)));

        public AudioBuffer Buffer
        {
            get { return (AudioBuffer)GetValue(BufferProperty); }
            set { SetValue(BufferProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Buffer.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BufferProperty =
            DependencyProperty.Register("Buffer", typeof(AudioBuffer), typeof(WaveFormView), new PropertyMetadata(default));

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (plotView == null)
            {
                return;
            }

            if (e.Property == MinimumProperty || e.Property == MaximumProperty)
            {
                UpdateMinimumAndMaximum();
            }

            if (e.Property == BufferProperty)
            {
                UpdatePlot();
            }
        }

        private void UpdateMinimumAndMaximum()
        {
            yAxis.Minimum = Minimum ?? double.NaN;
            yAxis.Maximum = Maximum ?? double.NaN;
            model.InvalidatePlot(false);
        }

        private void UpdatePlot()
        {
            plotView.InvalidatePlot();
            lineSeries.Points.Clear();

            if (Buffer == null)
            {
                return;
            }

            var buffer = Buffer.Floats;
            double sampleRate = Buffer.SampleRate;

            double lastX = 0;
            for (int i = 0; i < buffer.Length; i++)
            {
                double x = TimeSpanAxis.ToDouble(TimeSpan.FromSeconds((i + 1) / sampleRate));
                double y = buffer[i];
                lineSeries.Points.Add(new DataPoint(x, y));
                lastX = x;
            }

            xAxis.Maximum = lastX;

            model.InvalidatePlot(true);
        }

        private void saveImage_Click(object sender, RoutedEventArgs e)
        {
            var w = plotView.ActualWidth;
            var h = plotView.ActualHeight;
            var renderTargetBitmap = new RenderTargetBitmap((int)w, (int)h, 96, 96, PixelFormats.Pbgra32);
            renderTargetBitmap.Render(plotView);

            var sfd = new SaveFileDialog();
            sfd.Filter = "PNGファイル|*.png";
            sfd.FileName = "Plot.png";
            if (sfd.ShowDialog() ?? false)
            {
                PngBitmapEncoder pngImage = new PngBitmapEncoder();
                pngImage.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
                using (Stream fileStream = File.Create(sfd.FileName))
                {
                    pngImage.Save(fileStream);
                }
            }
        }
    }
}
