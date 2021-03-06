﻿using KSynthesizer.Filters;
using KSynthesizer.Sources;
using NAudio.CoreAudioApi;
using NAudio.Wave;
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
    /// OscillatorPanel.xaml の相互作用ロジック
    /// </summary>
    public partial class OscillatorPanel : UserControl
    {
        internal LastRecordFilter<PeriodicFunctionsSource> Osc1 => osc1.Source;

        internal LastRecordFilter<PeriodicFunctionsSource> Osc2 => osc2.Source;

        public event EventHandler FrequencyUpdated;

        public OscillatorPanel()
        {
            InitializeComponent();
        }

        public void Pause()
        {
            osc1.StopPreview();
            osc2.StopPreview();
        }

        public void Play()
        {
            osc1.StartPreview();
            osc2.StartPreview();
        }

        private void osc_FrequencyUpdated(object sender, EventArgs e)
        {
            FrequencyUpdated?.Invoke(this, e);
        }
    }
}
