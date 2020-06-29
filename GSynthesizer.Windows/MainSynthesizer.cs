using GSynthesizer.Windows.Services;
using KSynthesizer;
using KSynthesizer.Filters;
using KSynthesizer.Midi;
using KSynthesizer.NAudio;
using KSynthesizer.Soundio;
using Melanchall.DryWetMidi.Core;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace GSynthesizer
{
    public class MainSynthesizer : BindableBase
    {
        public MainSynthesizer()
        {
            SynthesizerRecorder = new SwapFilter<RecordFilter>(source => new RecordFilter(source), filter =>
            {
                filter.StopRecording();
                Recording.Cancel();
            });

            MidiPlayer = new MidiPlayer(Synthesizer.Format.SampleRate, Synthesizer);
            MidiPlayer.OscillatorConfigs.Add(new Oscillator() { Function = KSynthesizer.Sources.FunctionType.Sin, Volume = 0.3f });
            MidiPlayer.Finished += MidiPlayer_Finished;
        }

        private void MidiPlayer_Finished(object sender, EventArgs e)
        {
            MidiRestorationCallback?.Invoke();
            MidiRestorationCallback = null;
        }

        public event EventHandler<InterceptedEventArgs> Intercepted;

        public bool Initialized { get; private set; } = false;

        public Synthesizer Synthesizer { get; } = new Synthesizer(DeviceSettings.SharedFormat.SampleRate);

        public SwapFilter<RecordFilter> SynthesizerRecorder { get; private set; }

        public MidiPlayer MidiPlayer { get; }

        private IAudioOutput CurrentOutput { get; set; }

        private InterceptFilter SynthesizerIntercepter { get; set; }

        private Action MidiRestorationCallback { get; set; }

        private RecordingService Recording { get; } = new RecordingService();

        public bool Play(IAudioOutput output)
        {
            switch(output)
            {
                case WasapiOutput wasapi:
                    var outputSource = InitOutput(Synthesizer);
                    wasapi.Source = outputSource;
                    CurrentOutput = output;
                    break;
                default:
                    Initialized = false;
                    return false;
            }

            Initialized = true;
            return true;
        }

        public void Stop()
        {
            switch (CurrentOutput)
            {
                case WasapiOutput wasapi:
                    wasapi.Source = null;
                    break;
                default:
                    break;
            }
            Initialized = false;
        }

        public void OpenMidi(string path)
        {
            MidiPlayer.Open(MidiFile.Read(path));

            switch (CurrentOutput)
            {
                case WasapiOutput wasapi:
                    var oldSource = wasapi.Source;
                    var output = InitOutput(MidiPlayer);
                    wasapi.Source = output;

                    MidiRestorationCallback = () => wasapi.Source = oldSource;
                    break;
                default:
                    break;
            }
        }

        private IAudioSource InitOutput(IAudioSource source)
        {
            if (SynthesizerIntercepter != null)
            {
                SynthesizerIntercepter.Intercepted -= SynthesizerIntercepter_Intercepted;
            }

            SynthesizerIntercepter = new InterceptFilter(source);
            SynthesizerRecorder.Source = SynthesizerIntercepter;
            SynthesizerIntercepter.Intercepted += SynthesizerIntercepter_Intercepted;

            return SynthesizerRecorder;
        }

        public bool StartRecording()
        {
            var recorder = SynthesizerRecorder.GeneratedSource;
            if (recorder != null)
            {
                Recording.Start(recorder);
                return true;
            }

            return false;
        }

        public async Task StopRecording(string outputPath)
        {
            await Recording.StopAsync(outputPath);
        }

        private void SynthesizerIntercepter_Intercepted(object sender, InterceptedEventArgs e)
        {
            Intercepted?.Invoke(this, e);
        }
    }
}
