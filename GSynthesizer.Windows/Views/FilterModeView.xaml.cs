using KSynthesizer.Filters;
using System.Windows;
using System.Windows.Controls;

namespace GSynthesizer.Windows.Views
{
    /// <summary>
    /// Interaction logic for FilterModeView
    /// </summary>
    public partial class FilterModeView : UserControl
    {
        public FilterModeView()
        {
            InitializeComponent();
        }

        public event RoutedPropertyChangedEventHandler<FrequencyFilterMode> FilterModeChanged;

        public FrequencyFilterMode FilterMode
        {
            get { return (FrequencyFilterMode)GetValue(FilterModeProperty); }
            set
            {
                var oldValue = FilterMode;
                SetValue(FilterModeProperty, value);

                switch (value)
                {
                    case FrequencyFilterMode.Disable:
                        disabled.IsChecked = true;
                        break;
                    case FrequencyFilterMode.Lowpass:
                        lpf.IsChecked = true;
                        break;
                    case FrequencyFilterMode.Highpass:
                        hpf.IsChecked = true;
                        break;
                    case FrequencyFilterMode.Bandpass:
                        bpf.IsChecked = true;
                        break;
                }

                FilterModeChanged?.Invoke(this, new RoutedPropertyChangedEventArgs<FrequencyFilterMode>(oldValue, value));
            }
        }

        // Using a DependencyProperty as the backing store for FilterMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FilterModeProperty =
            DependencyProperty.Register("FilterMode", typeof(FrequencyFilterMode), typeof(FilterModeView), new PropertyMetadata(FrequencyFilterMode.Disable));

        private void disabled_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            FilterMode = FrequencyFilterMode.Disable;
        }

        private void lpf_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            FilterMode = FrequencyFilterMode.Lowpass;
        }

        private void hpf_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            FilterMode = FrequencyFilterMode.Highpass;
        }

        private void bpf_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            FilterMode = FrequencyFilterMode.Bandpass;
        }
    }
}
