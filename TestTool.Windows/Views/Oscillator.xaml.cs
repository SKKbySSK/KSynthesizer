using System;
using System.Collections.Generic;
using System.Linq;
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
using KSynthesizer.Sources;

namespace TestTool.Windows.Views
{
    /// <summary>
    /// Oscillator.xaml の相互作用ロジック
    /// </summary>
    public partial class Oscillator : UserControl
    {
        private Dictionary<FunctionType, RadioButton> buttons = new Dictionary<FunctionType, RadioButton>();

        public event EventHandler FrequencyUpdated;

        public Oscillator()
        {
            InitializeComponent();

            Function = FunctionType.Sin;
            foreach (var func in Enum.GetValues(typeof(FunctionType)))
            {
                var radio = new RadioButton()
                {
                    Content = func,
                    IsChecked = (FunctionType)func == Source.Source.Function,
                    Margin = new Thickness(5),
                };
                radio.Click += (sender, e) =>
                {
                    Function = (FunctionType)func;
                };

                oscStack.Children.Add(radio);
                buttons[(FunctionType)func] = radio;
            }

            wave.PlotSource = Source;
        }

        public FunctionType Function
        {
            get => Source.Source.Function;
            set => Source.Source.Function = value;
        }

        internal LastRecordFilter<PeriodicFunctionsSource> Source { get; } = new LastRecordFilter<PeriodicFunctionsSource>(new PeriodicFunctionsSource(44100));

        private void LabelSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Source.Source.SetFrequency((float)e.NewValue);
            FrequencyUpdated?.Invoke(this, EventArgs.Empty);
        }

        private void dcSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Source.Source.DcValue = (float)e.NewValue;
        }

        private void rectDuty_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Source.Source.RectDuty = (float)e.NewValue;
        }

        public void StartPreview()
        {
            wave.Play();
        }

        public void StopPreview()
        {
            wave.Pause();
        }
    }
}
