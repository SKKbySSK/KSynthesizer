using HandyControl.Controls;
using KSynthesizer.Midi;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : GlowWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            KeyDown += MainWindow_KeyDown;
            KeyUp += MainWindow_KeyUp;
        }

        private void MainWindow_KeyUp(object sender, KeyEventArgs e)
        {
            panel.OnPhysicalKeyUp(e.Key);
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            panel.OnPhysicalKeyDown(e.Key);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            panel.Dispose();
        }

        private void openMidi_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "MIDIファイル|*.midi;*.mid";
            if (ofd.ShowDialog() ?? false)
            {
                panel.SetCustomMidiSource(new MidiPlayer(44100, Melanchall.DryWetMidi.Core.MidiFile.Read(ofd.FileName)));
            }
        }
    }
}
