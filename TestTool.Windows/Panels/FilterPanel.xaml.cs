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

namespace TestTool.Windows.Panels
{
    /// <summary>
    /// FilterPanel.xaml の相互作用ロジック
    /// </summary>
    public partial class FilterPanel : UserControl
    {
        private LastRecordFilter<FrequencyFilter> filter;

        internal LastRecordFilter<FrequencyFilter> Filter 
        {
            get => filter; 
            set
            {
                filter = value;
                UpdateFilter();
            }
        }

        public FilterPanel()
        {
            InitializeComponent();
        }

        private void LabelSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateFilter();
        }

        private void filter_Checked(object sender, RoutedEventArgs e)
        {
            UpdateFilter();
        }

        private void UpdateFilter()
        {
            float low = (float)lowFreq.Value;
            float high = (float)highFreq.Value;

            if (lpf.IsChecked ?? false)
            {
                Filter?.Source.SetLowpassMode(low);
            }

            if (hpf.IsChecked ?? false)
            {
                Filter?.Source.SetHighpassMode(high);
            }

            if (bpf.IsChecked ?? false)
            {
                Filter?.Source.SetBandpassMode(low, high);
            }
        }
    }
}
