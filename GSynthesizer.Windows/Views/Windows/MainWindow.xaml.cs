using GSynthesizer.ViewModels.Windows;
using KSynthesizer.Filters;
using KSynthesizer.Sources;
using Microsoft.Win32;
using System.Windows;

namespace GSynthesizer.Views.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var vm = (MainWindowViewModel)DataContext;
            vm.Dispose();
        }

        private void openMidi_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "midファイル|*.mid";
            if (ofd.ShowDialog() ?? false)
            {
                var vm = (MainWindowViewModel)DataContext;
                if (vm.OpenMidi.CanExecute(ofd.FileName))
                {
                    vm.OpenMidi.Execute(ofd.FileName);
                }
            }
        }

        private void rec_Click(object sender, RoutedEventArgs e)
        {
            var vm = (MainWindowViewModel)DataContext;
            if (vm.StartRecording())
            {
                rec.Visibility = Visibility.Hidden;
            }
        }

        private async void stop_Click(object sender, RoutedEventArgs e)
        {
            stop.IsEnabled = false;
            var sfd = new SaveFileDialog();
            sfd.Filter = "WAVEファイル|*.wav";

            if (sfd.ShowDialog() ?? false)
            {
                var vm = (MainWindowViewModel)DataContext;
                await vm.StopRecording(sfd.FileName);
            }

            rec.Visibility = Visibility.Visible;
            stop.IsEnabled = true;
        }
    }
}
