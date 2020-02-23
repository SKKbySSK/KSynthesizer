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
    /// <summary>
    /// DurationDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class DurationDialog : Window
    {
        public TimeSpan Duration { get; set; }

        public event EventHandler Saved;

        public DurationDialog()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(durationBox.Text, out var durationSec) && durationSec >= 0)
            {
                Duration = TimeSpan.FromSeconds(durationSec);
                Saved?.Invoke(this, EventArgs.Empty);
                Close();
            }
            else
            {
                MessageBox.Show("秒数を正の数で入力してください");
            }
        }
    }
}
