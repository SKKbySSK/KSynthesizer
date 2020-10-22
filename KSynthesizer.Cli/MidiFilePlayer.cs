// using System;
// using System.Collections.Generic;
// using System.Threading;
// using System.Threading.Tasks;
// using KSynthesizer.Midi;
// using KSynthesizer.Soundio;
// using KSynthesizer.Sources;
// using Melanchall.DryWetMidi.Core;
//
// namespace KSynthesizer.Cli
// {
//     class MidiFilePlayer
//     {
//         public SoundioOutput Output { get; private set; } = new SoundioOutput();
//
//         public Synthesizer Synthesizer { get; private set; }
//         
//         public MidiPlayer MidiPlayer { get; private set; }
//
//         public double MinCutoffLow { get; set; } = 20;
//
//         public double MaxCutoffLow { get; set; } = 2000;
//
//         public double MinCutoffHigh { get; set; } = 440;
//
//         public double MaxCutoffHigh { get; set; } = 4000;
//
//         public double ThumbStep { get; set; } = 0.1;
//
//         public float CutoffLow
//         {
//             get
//             {
//                 var low = thumbMapping[KeyboardThumb.CutoffLow] * (MaxCutoffLow - MinCutoffLow);
//                 var lowOffset = low + MinCutoffLow;
//                 return (float) lowOffset;
//             }
//         }
//
//         public float CutoffHigh
//         {
//             get
//             {
//                 var high = thumbMapping[KeyboardThumb.CutoffHigh] * (MaxCutoffHigh - MinCutoffHigh);
//                 var highOffset = high + MinCutoffHigh;
//                 return (float) highOffset;
//             }
//         }
//
//         public bool DebugPrint { get; set; } = true;
//         
//         private Dictionary<KeyboardThumb, double> thumbMapping = new Dictionary<KeyboardThumb, double>
//         {
//             { KeyboardThumb.CutoffLow, 0.0 },
//             { KeyboardThumb.CutoffHigh, 1.0 },
//             { KeyboardThumb.MasterVolume, 1.0 },
//         };
//
//         public void Initialize(SoundIOSharp.SoundIODevice device, AudioFormat format, FunctionType functionType = FunctionType.Sin, float volume = 0.2f)
//         {
//             if (Output == null)
//             {
//                 Output = new SoundioOutput();
//             }
//             
//             Output.DesiredLatency = TimeSpan.FromMilliseconds(100);
//             Output.Initialize(device, format);
//             Output.Buffer += OutputOnBuffer;
//
//             Synthesizer = new Synthesizer(format.SampleRate, 50);
//             
//             MidiPlayer = new MidiPlayer(format.SampleRate, Synthesizer);
//             MidiPlayer.OscillatorConfigs.Add(new Oscillator(functionType, 0, volume));
//
//             if (DebugPrint)
//             {
//                 Console.WriteLine($"Initialized : {device.Name}, {format.SampleRate}Hz, {Output.ActualLatency.TotalMilliseconds}ms");
//             }
//         }
//
//         private void OutputOnBuffer(object? sender, OutputBufferEventArgs e)
//         {
//             e.Buffer = MidiPlayer.Next(e.Size);
//         }
//
//         public Task Run(MidiFile file, IEventInput input, CancellationToken cancellationToken)
//         {
//             return Task.Run(() => RunInternal(file, input, cancellationToken));
//         }
//
//         private void RunInternal(MidiFile file, IEventInput input, CancellationToken cancellationToken)
//         {
//             if (Output == null)
//             {
//                 throw new Exception("SynthesizerPlayer was not initialized");
//             }
//
//             MidiPlayer.Open(file);
//             Output.Play();
//             
//             bool exit = false;
//             while(!input.Exit && !exit && !cancellationToken.IsCancellationRequested)
//             {
//                 var result = input.Read();
//                 if (input.Event == null)
//                 {
//                     continue;
//                 }
//
//                 switch (result)
//                 {
//                     case EventReadResult.Failed:
//                         exit = true;
//                         Console.WriteLine("Failed to read next buffer");
//                         continue;
//                     case EventReadResult.Timeout:
//                         continue;
//                 }
//
//                 if (DebugPrint)
//                 {
//                     Console.WriteLine($"Event : {input.Event}");
//                 }
//                 switch (input.Event.Type)
//                 {
//                     case ToneEventType.Thumb:
//                         var thumb = (ToneThumbEvent) input.Event;
//                         handleThumbEvent(thumb);
//                         break;
//                     case ToneEventType.Filter:
//                         var filter = (ToneFilterEvent) input.Event;
//                         handleFilterEvent(filter);
//                         break;
//                     default:
//                         Console.WriteLine($"Unsupported event type : {input.Event.Type}");
//                         break;
//                 }
//             }
//
//             Output.Buffer -= OutputOnBuffer;
//             Output.Stop();
//             
//             if (DebugPrint)
//             {
//                 Console.WriteLine("Output has stopped");
//             }
//             
//             Output.Dispose();
//
//             if (DebugPrint)
//             {
//                 Console.WriteLine("Thread exited successfully");
//             }
//         }
//
//         private void handleFilterEvent(ToneFilterEvent e)
//         {
//             if (DebugPrint)
//             {
//                 Console.WriteLine($"Filter : {e.Mode}");
//             }
//             
//             switch (e.Mode)
//             {
//                 case FilterMode.Disabled:
//                     Synthesizer.FrequencyFilter.Disable();
//                     break;
//                 case FilterMode.Lowpass:
//                     Synthesizer.FrequencyFilter.SetLowpassMode(CutoffLow);
//                     break;
//                 case FilterMode.Highpass:
//                     Synthesizer.FrequencyFilter.SetHighpassMode(CutoffHigh);
//                     break;
//                 case FilterMode.Bandpass:
//                     Synthesizer.FrequencyFilter.SetBandpassMode(CutoffLow, CutoffHigh);
//                     break;
//             }
//         }
//
//         private void handleThumbEvent(ToneThumbEvent e)
//         {
//             var thumb = (KeyboardThumb) e.Index;
//             
//             var val = thumbMapping[thumb];
//             switch (e.Direction)
//             {
//                 case ThumbDirection.Clockwise:
//                     val += ThumbStep;
//                     break;
//                 case ThumbDirection.CounterClockwise:
//                     val -= ThumbStep;
//                     break;
//             }
//             val = Math.Min(1, Math.Max(0, val));
//
//             thumbMapping[thumb] = val;
//
//             switch (thumb)
//             {
//                 case KeyboardThumb.CutoffLow:
//                 case KeyboardThumb.CutoffHigh:
//                     updateFrequencyFilter();
//                     break;
//                 case KeyboardThumb.MasterVolume:
//                     Synthesizer.MixerVolume = (float)val;
//                     break;
//             }
//         }
//
//         private void updateFrequencyFilter()
//         {
//             if (DebugPrint)
//             {
//                 Console.WriteLine($"Cutoff : Low {CutoffLow}Hz, High {CutoffHigh}Hz");
//             }
//             
//             Synthesizer.FrequencyFilter.ChangeFrequency(CutoffLow, CutoffHigh);
//         }
//     }
// }