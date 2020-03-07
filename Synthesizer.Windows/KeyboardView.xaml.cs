using KSynthesizer;
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
using System.Linq;

namespace Synthesizer.Windows
{
    public class KeyView : UserControl
    {
        private bool sharp;

        public IonianScale Scale { get; set; }

        public bool Sharp
        { 
            get => sharp;
            set
            {
                sharp = value;
                UpdateColor();
            }
        }

        public bool IsPressed { get; private set; } = false;

        public float Octave { get; set; }

        public KeyView()
        {
            OnKeyUp();
            BorderThickness = new Thickness(1);
            BorderBrush = new SolidColorBrush(Color.FromRgb(120, 120, 120));
            UpdateColor();
        }

        public void OnKeyDown()
        {
            IsPressed = true;
            UpdateColor();
        }

        public void OnKeyUp()
        {
            IsPressed = false;
            UpdateColor();
        }

        private void UpdateColor()
        {
            if (Sharp)
            {
                if (IsPressed)
                {
                    Background = new SolidColorBrush(Color.FromRgb(20, 20, 20));
                }
                else
                {
                    Background = new SolidColorBrush(Color.FromRgb(50, 50, 50));
                }
            }
            else
            {
                if (IsPressed)
                {
                    Background = new SolidColorBrush(Color.FromRgb(200, 200, 200));
                }
                else
                {
                    Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                }
            }
        }
    }

    /// <summary>
    /// KeyboardView.xaml の相互作用ロジック
    /// </summary>
    public partial class KeyboardView : UserControl
    {
        int lastChildrenCount = 0;
        List<KeyView> visibleKeys = new List<KeyView>();
        public event EventHandler<ValueEventArgs<IonianTone>> ToneKeyDown;
        public event EventHandler<ValueEventArgs<IonianTone>> ToneKeyUp;

        public KeyboardView()
        {
            InitializeComponent();

        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            if (sizeInfo.WidthChanged)
            {
                RefreshChildren(sizeInfo.NewSize.Width);
            }
        }

        public int Octave { get; set; } = 2;

        public double KeyWidth { get; set; } = 70;

        public double SharpKeyWidth { get; set; } = 40;

        private void Key_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsEnabled)
            {
                return;
            }
            var view = (KeyView)sender;
            view.OnKeyDown();
            ToneKeyDown?.Invoke(this, new ValueEventArgs<IonianTone>(new IonianTone()
            {
                Scale = view.Scale,
                Sharp = view.Sharp,
                Octave = view.Octave,
            }));
        }

        private void Key_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!IsEnabled)
            {
                return;
            }
            var view = (KeyView)sender;
            view.OnKeyUp();
            ToneKeyUp?.Invoke(this, new ValueEventArgs<IonianTone>(new IonianTone()
            {
                Scale = view.Scale,
                Sharp = view.Sharp,
                Octave = view.Octave,
            }));
        }

        private void Key_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!IsEnabled)
            {
                return;
            }
            var view = (KeyView)sender;
            if (!view.IsPressed && Mouse.LeftButton == MouseButtonState.Pressed)
            {
                view.OnKeyDown();
                ToneKeyDown?.Invoke(this, new ValueEventArgs<IonianTone>(new IonianTone()
                {
                    Scale = view.Scale,
                    Sharp = view.Sharp,
                    Octave = view.Octave,
                }));
            }
        }

        private void Key_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!IsEnabled)
            {
                return;
            }
            var view = (KeyView)sender;
            if (view.IsPressed)
            {
                view.OnKeyUp();
                ToneKeyUp?.Invoke(this, new ValueEventArgs<IonianTone>(new IonianTone()
                {
                    Scale = view.Scale,
                    Sharp = view.Sharp,
                    Octave = view.Octave,
                }));
            }
        }

        public void SimulateKeyDown(IonianTone tone)
        {
            if (!IsEnabled)
            {
                return;
            }
            var view = visibleKeys.FirstOrDefault(view => view.Scale == tone.Scale && view.Sharp == tone.Sharp && view.Octave == tone.Octave);
            if (view != null)
            {
                if (view.IsPressed)
                {
                    return;
                }
                view.OnKeyDown();
            }

            ToneKeyDown?.Invoke(this, new ValueEventArgs<IonianTone>(tone));
        }

        public void SimulateKeyUp(IonianTone tone)
        {
            if (!IsEnabled)
            {
                return;
            }
            var view = visibleKeys.FirstOrDefault(view => view.Scale == tone.Scale && view.Sharp == tone.Sharp && view.Octave == tone.Octave);
            if (view != null)
            {
                if (!view.IsPressed)
                {
                    return;
                }
                view.OnKeyUp();
            }

            ToneKeyUp?.Invoke(this, new ValueEventArgs<IonianTone>(tone));
        }

        private void RefreshChildren(double width)
        {
            int count = (int)(width / KeyWidth);
            if (lastChildrenCount == count)
            {
                return;
            }
            lastChildrenCount = count;
            visibleKeys.Clear();

            foreach (KeyView key in grid.Children)
            {
                key.MouseLeftButtonDown -= Key_MouseLeftButtonDown;
                key.MouseLeftButtonUp -= Key_MouseLeftButtonUp;
                key.MouseEnter -= Key_MouseEnter;
                key.MouseLeave -= Key_MouseLeave;
            }
            grid.Children.Clear();
            grid.ColumnDefinitions.Clear();

            foreach (KeyView key in sharpGrid.Children)
            {
                key.MouseLeftButtonDown -= Key_MouseLeftButtonDown;
                key.MouseLeftButtonUp -= Key_MouseLeftButtonUp;
                key.MouseEnter -= Key_MouseEnter;
                key.MouseLeave -= Key_MouseLeave;
            }
            sharpGrid.Children.Clear();
            sharpGrid.ColumnDefinitions.Clear();

            var scales = Enum.GetValues(typeof(IonianScale)).Cast<IonianScale>().ToArray();
            int col = 0;
            int sharpCol = 0;
            int scaleIndex = 0;
            int octave = Octave;
            double halfSharpWidth = SharpKeyWidth / 2;
            sharpGrid.Margin = new Thickness(halfSharpWidth, 0, 0, 0);

            for (int i = 0; count > i; i++)
            {
                var scale = scales[scaleIndex++];
                var key = new KeyView() { Scale = scale, Octave = octave };
                key.MouseLeftButtonDown += Key_MouseLeftButtonDown;
                key.MouseLeftButtonUp += Key_MouseLeftButtonUp;
                key.MouseEnter += Key_MouseEnter;
                key.MouseLeave += Key_MouseLeave;
                Grid.SetColumn(key, col++);
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(KeyWidth) });
                grid.Children.Add(key);
                visibleKeys.Add(key);

                sharpGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(KeyWidth - SharpKeyWidth) });
                sharpCol++;
                sharpGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(SharpKeyWidth) });
                if (i > 0)
                {
                    if (scale != IonianScale.B && scale != IonianScale.E)
                    {
                        var sharp = new KeyView() { Scale = scale, Octave = octave, Sharp = true };
                        sharp.MouseLeftButtonDown += Key_MouseLeftButtonDown;
                        sharp.MouseLeftButtonUp += Key_MouseLeftButtonUp;
                        sharp.MouseEnter += Key_MouseEnter;
                        sharp.MouseLeave += Key_MouseLeave;
                        Grid.SetColumn(sharp, sharpCol);
                        sharpGrid.Children.Add(sharp);
                        visibleKeys.Add(sharp);
                    }
                }
                sharpCol++;

                if (scaleIndex == scales.Length)
                {
                    scaleIndex = 0;
                    octave++;
                }
            }

            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            sharpGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
        }
    }
}
