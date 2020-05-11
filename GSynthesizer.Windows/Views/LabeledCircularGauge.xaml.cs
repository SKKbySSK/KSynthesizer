using Reactive.Bindings;
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

namespace GSynthesizer.Windows.Views
{
    /// <summary>
    /// Interaction logic for LabeledCircularGauge.xaml
    /// </summary>
    public partial class LabeledCircularGauge : UserControl
    {
        public LabeledCircularGauge()
        {
            InitializeComponent();
        }

        public ReactiveProperty<double> Maximum { get; } = new ReactiveProperty<double>(1);

        public ReactiveProperty<double> Minimum { get; } = new ReactiveProperty<double>(0);

        public ReactiveProperty<double> Value { get; } = new ReactiveProperty<double>(1);
    }
}
