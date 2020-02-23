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

namespace TestTool.Windows.Views
{
    /// <summary>
    /// LabelSlider.xaml の相互作用ロジック
    /// </summary>
    public partial class LabelSlider : UserControl
    {
        private string format = "'int'";

        public string Format 
        { 
            get => format; 
            set
            {
                format = value;
                slider_ValueChanged(slider, new RoutedPropertyChangedEventArgs<double>(slider.Value, slider.Value));
            }
        }

        public double Value
        {
            get => slider.Value;
            set => slider.Value = value;
        }

        public double Maximum
        {
            get => slider.Maximum;
            set => slider.Maximum = value;
        }

        public double Minimum
        {
            get => slider.Minimum;
            set => slider.Minimum = value;
        }

        public double SmallChange
        {
            get => slider.SmallChange;
            set => slider.SmallChange = value;
        }

        public double LargeChange
        {
            get => slider.LargeChange;
            set => slider.LargeChange = value;
        }

        public event RoutedPropertyChangedEventHandler<double> ValueChanged;

        public LabelSlider()
        {
            InitializeComponent();
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int val = (int)Math.Round(Value);
            var text = Format.Replace("'int'", val.ToString()).Replace("'float'", Math.Round(Value, 2).ToString());
            label.Content = text;

            ValueChanged?.Invoke(this, e);
        }
    }
}
