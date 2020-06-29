using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GSynthesizer.Windows.Views
{
    /// <summary>
    /// Interaction logic for KnobView
    /// </summary>
    public partial class KnobView : UserControl
    {
        public KnobView()
        {
            InitializeComponent();
        }

        public event RoutedPropertyChangedEventHandler<double> ValueChanged;

        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Minimum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(double), typeof(KnobView), new PropertyMetadata(0.0));

        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Maximum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(double), typeof(KnobView), new PropertyMetadata(1.0));

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set
            {
                var oldValue = Value;
                SetValue(ValueProperty, value);
                ValueChanged?.Invoke(this, new RoutedPropertyChangedEventArgs<double>(oldValue, value));
            }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(KnobView), new PropertyMetadata(0.0));

        public Brush Fill
        {
            get { return (Brush)GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Fill.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FillProperty =
            DependencyProperty.Register("Fill", typeof(Brush), typeof(KnobView), new PropertyMetadata(new SolidColorBrush(Colors.White)));

        public Brush LineFill
        {
            get { return (Brush)GetValue(LineFillProperty); }
            set { SetValue(LineFillProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LineFill.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LineFillProperty =
            DependencyProperty.Register("LineFill", typeof(Brush), typeof(KnobView), new PropertyMetadata(new SolidColorBrush(Colors.Red)));

        public double MinAngle
        {
            get { return (double)GetValue(MinAngleProperty); }
            set { SetValue(MinAngleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MinAngle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinAngleProperty =
            DependencyProperty.Register("MinAngle", typeof(double), typeof(KnobView), new PropertyMetadata(200.0));

        public double MaxAngle
        {
            get { return (double)GetValue(MaxAngleProperty); }
            set { SetValue(MaxAngleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaxAngle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxAngleProperty =
            DependencyProperty.Register("MaxAngle", typeof(double), typeof(KnobView), new PropertyMetadata(360.0 + 160.0));

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            var size = Math.Min(sizeInfo.NewSize.Width, sizeInfo.NewSize.Height);
            var half = size / 2;
            line.X1 = half;
            line.X2 = half;
            line.Y1 = 0;
            line.Y2 = half;
            lineRotate.CenterX = half;
            lineRotate.CenterY = half;
            grid.Width = size;
            grid.Height = size;

            var delta = Maximum - Minimum;
            lineRotate.Angle = (Value / delta) * (MaxAngle - MinAngle) + MinAngle;
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (line == null)
            {
                return;
            }

            line.Stroke = LineFill;
            ellipse.Fill = Fill;
            var delta = Maximum - Minimum;
            lineRotate.Angle = (Value / delta) * (MaxAngle - MinAngle) + MinAngle;
        }

        private bool handleMouseMove = false;
        private Point lastPoint;

        private void grid_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            handleMouseMove = true;
            lastPoint = e.GetPosition(grid);
            grid.CaptureMouse();
        }

        private void grid_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            handleMouseMove = false;
            grid.ReleaseMouseCapture();
        }

        private void grid_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (handleMouseMove)
            {
                var point = e.GetPosition(grid);
                var delta = point - lastPoint;
                var move = delta.Y / 300 * (Maximum - Minimum);
                var newValue = Value - move;
                Value = Math.Min(Maximum, Math.Max(Minimum, newValue));

                lastPoint = point;
            }
        }
    }
}
