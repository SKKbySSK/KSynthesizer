using KSynthesizer.Filters;
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

namespace Synthesizer.Windows
{
    /// <summary>
    /// FrequencyPanel.xaml の相互作用ロジック
    /// </summary>
    public partial class FrequencyPanel : UserControl
    {
        private FrequencyFilter filter;
        private bool initializing = true;

        public FrequencyPanel()
        {
            InitializeComponent();
            initializing = false;
        }

        public FrequencyFilter Filter 
        { 
            get => filter; 
            set
            {
                filter = value;
                UpdateFilter();
            }
        }

        private void lpf_Checked(object sender, RoutedEventArgs e)
        {
            if (initializing) return;
            lowStack.Visibility = Visibility.Visible;
            highStack.Visibility = Visibility.Hidden;
            UpdateLabel();
            UpdateFilter();
        }

        private void hpf_Checked(object sender, RoutedEventArgs e)
        {
            if (initializing) return;
            lowStack.Visibility = Visibility.Hidden;
            highStack.Visibility = Visibility.Visible;
            UpdateLabel();
            UpdateFilter();
        }

        private void bpf_Checked(object sender, RoutedEventArgs e)
        {
            if (initializing) return;
            lowStack.Visibility = Visibility.Visible;
            highStack.Visibility = Visibility.Visible;
            if (highCutoff.Value - 1 < lowCutoff.Value && (bpf.IsChecked ?? false))
            {
                highCutoff.ValueChanged -= highCutoff_ValueChanged;
                highCutoff.Value = lowCutoff.Value + 1;
                highCutoff.ValueChanged += highCutoff_ValueChanged;
            }
            UpdateLabel();
            UpdateFilter();
        }

        private void UpdateFilter()
        {
            if (Filter == null) return;
            if (initializing) return;
            float low = (float)(lowCutoff.Value / 100) * (Filter.Format.SampleRate / 4);
            float high = (float)(highCutoff.Value / 100) * (Filter.Format.SampleRate / 4);

            if (lpf.IsChecked ?? false)
            {
                filter.SetLowpassMode(low);
            }

            if (hpf.IsChecked ?? false)
            {
                filter.SetHighpassMode(high);
            }

            if (bpf.IsChecked ?? false)
            {
                filter.SetBandpassMode(low, high);
            }
        }

        private void lowCutoff_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (initializing) return;
            if (highCutoff.Value - 1 < lowCutoff.Value && (bpf.IsChecked ?? false))
            {
                highCutoff.ValueChanged -= highCutoff_ValueChanged;
                highCutoff.Value = lowCutoff.Value + 1;
                highCutoff.ValueChanged += highCutoff_ValueChanged;
            }
            UpdateLabel();
            UpdateFilter();
        }

        private void highCutoff_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (initializing) return;
            if (highCutoff.Value - 1 < lowCutoff.Value && (bpf.IsChecked ?? false))
            {
                lowCutoff.ValueChanged -= highCutoff_ValueChanged;
                lowCutoff.Value = highCutoff.Value - 1;
                lowCutoff.ValueChanged += highCutoff_ValueChanged;
            }
            UpdateLabel();
            UpdateFilter();
        }

        private void UpdateLabel()
        {
            float low = (float)(lowCutoff.Value / 100) * (Filter.Format.SampleRate / 4);
            float high = (float)(highCutoff.Value / 100) * (Filter.Format.SampleRate / 4);
            lowLabel.Content = $"{(int)low}Hz";
            highLabel.Content = $"{(int)high}Hz";
        }
    }
}
