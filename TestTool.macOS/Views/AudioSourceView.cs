using System;
using System.Timers;
using AppKit;
using CoreFoundation;
using CoreGraphics;
using Foundation;
using KSynthesizer.Sources;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Xamarin.Mac;

namespace TestTool.macOS.Views
{
    public enum SourceFunction
    {
        Sin,
    }
    
    [Register(nameof(AudioSourceView))]
    public class AudioSourceView : NSView
    {
        public AudioSourceView()
        {
            Function = SourceFunction.Sin;
        }

        private PeriodicSourceBase source;
        private SourceFunction _function;
        private float _frequency;
        private Timer _timer = new Timer();
        private PlotView _plot;
        private PlotModel _model = new PlotModel();
        private LineSeries _lineSeries = new LineSeries();

        public SourceFunction Function
        {
            get => _function;
            set
            {
                _function = value;
                source = null;

                switch (value)
                {
                    case SourceFunction.Sin:
                        source = new SinSource(100, 100);
                        break;
                    default:
                        throw new Exception("Unknown Audio Source Type : " + value);
                }
            }
        }

        public float Frequency
        {
            get => _frequency;
            set
            {
                _frequency = value;
                source?.SetFrequency(value);
            }
        }

        public override void ViewDidMoveToSuperview()
        {
            base.ViewDidMoveToSuperview();
            _plot = new PlotView(Bounds);
            AddSubview(_plot);
            _plot.TranslatesAutoresizingMaskIntoConstraints = false;
            NSLayoutConstraint.ActivateConstraints(
                new []
                {
                    _plot.TopAnchor.ConstraintEqualToAnchor(TopAnchor),
                    _plot.RightAnchor.ConstraintEqualToAnchor(RightAnchor),
                    _plot.BottomAnchor.ConstraintEqualToAnchor(BottomAnchor),
                    _plot.LeftAnchor.ConstraintEqualToAnchor(LeftAnchor),
                }
                );
            _plot.WantsLayer = true;
            _plot.Layer.BackgroundColor = new CGColor(0, 1, 0);
            NeedsLayout = true;
            
            _model.Axes.Add(new LinearAxis(){Position = AxisPosition.Bottom, Minimum = 1, Maximum = 100});
            _model.Axes.Add(new LinearAxis(){Position = AxisPosition.Left, Minimum = -1.2, Maximum = 1.2});
            _model.Series.Add(_lineSeries);
            
            _plot.Model = _model;
            _timer.Elapsed += TimerOnElapsed;
            _timer.Interval = 0.1;
            _timer.Start();
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            DispatchQueue.MainQueue.DispatchAsync(() =>
            {
                _lineSeries.Points.Clear();
            
                var buffer = source?.Next();
                if (buffer == null)
                {
                    _plot.InvalidatePlot();
                    return;
                }

                for (int i = 0; buffer.Length > i; i++)
                {
                    _lineSeries.Points.Add(new DataPoint(i + 1, buffer[i]));
                }
                _plot.InvalidatePlot(true);
            });
        }

        public override void RemoveFromSuperview()
        {
            base.RemoveFromSuperview();
            _timer.Elapsed -= TimerOnElapsed;
            _timer.Stop();
        }
    }
}