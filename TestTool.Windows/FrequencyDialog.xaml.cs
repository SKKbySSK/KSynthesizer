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
using System.Windows.Shapes;

namespace TestTool.Windows
{
    public class FreqEventArgs : EventArgs
    {
        public double From { get;set; }
        
        public double To { get; set; }

        public double Step { get; set; }
    }

    /// <summary>
    /// FrequencyDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class FrequencyDialog : Window
    {
        public event EventHandler<FreqEventArgs> ExportClicked;

        public FrequencyDialog()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ExportClicked?.Invoke(this, new FreqEventArgs()
            {
                From = fromFreq.Value,
                To = toFreq.Value,
                Step = stepFreq.Value,
            });
            Close();
        }
    }
}
