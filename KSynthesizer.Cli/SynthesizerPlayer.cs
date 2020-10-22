using KSynthesizer.Soundio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KSynthesizer.Filters;
using KSynthesizer.Midi;
using KSynthesizer.Sources;
using Melanchall.DryWetMidi.Core;

namespace KSynthesizer.Cli
{
    struct ValueRange
    {
        public double Min;
        public double Max;

        public double GetRangeValue(double value, Func<double, double> converter = null)
        {
            var converted = converter == null ? value : converter(value);
            return converted * (Max - Min) + Min;
            // 27.5 * Math.Pow(2, octave);
        }
    }

    class LoopState<T>
    {
        public LoopState(T[] states)
        {
            States = states;
        }
        
        private int index = 0;
        
        public T[] States { get; }

        public T CurrentState => States[index];

        public void Next()
        {
            if (++index >= States.Length)
            {
                index = 0;
            }
        }
    }
    
    class SynthesizerPlayer
    {
        private readonly Dictionary<IonianTone, int> toneMap = new Dictionary<IonianTone, int>();

        public SoundioOutput Output { get; private set; } = new SoundioOutput();
        
        private RecordFilter Recorder { get; set; }

        public Synthesizer Synthesizer { get; private set; }
        
        public MidiPlayer MidiPlayer { get; private set; }

        public ValueRange AttackRange { get; set; } = new ValueRange()
        {
            Min = 0,
            Max = 1000,
        };

        public ValueRange DecayRange { get; set; } = new ValueRange()
        {
            Min = 0,
            Max = 1000,
        };

        public ValueRange SustainRange { get; set; } = new ValueRange()
        {
            Min = 0,
            Max = 1,
        };

        public ValueRange ReleaseRange { get; set; } = new ValueRange()
        {
            Min = 0,
            Max = 1000,
        };

        public ValueRange CutoffLowRange { get; set; } = new ValueRange()
        {
            Min = 20,
            Max = 2000,
        };

        public ValueRange CutoffHighRange { get; set; } = new ValueRange()
        {
            Min = 440,
            Max = 4000,
        };
        
        public LoopState<FilterMode> Filter { get; } = new LoopState<FilterMode>(new []
        {
            FilterMode.Disabled,
            FilterMode.Lowpass,
            FilterMode.Highpass,
            FilterMode.Bandpass,
        });
        
        public LoopState<FunctionType> Wave { get; } = new LoopState<FunctionType>(new []
        {
            FunctionType.Sin,
            FunctionType.Rect,
            FunctionType.Triangle,
            FunctionType.Sawtooth,
        });

        public double ThumbStep { get; set; } = 0.1;

        public float CutoffLow
        {
            get
            {
                var low = thumbMapping[ThumbType.CutoffLow];
                return (float) CutoffLowRange.GetRangeValue(low, ConvertFrequency);
            }
        }

        public float CutoffHigh
        {
            get
            {
                var high = thumbMapping[ThumbType.CutoffHigh];
                return (float) CutoffHighRange.GetRangeValue(high, ConvertFrequency);;
            }
        }

        public TimeSpan AttackDuration
        {
            get
            {
                var durMs = (float) AttackRange.GetRangeValue(thumbMapping[ThumbType.Attack]);
                return TimeSpan.FromMilliseconds(durMs);
            }
        }

        public TimeSpan DecayDuration
        {
            get
            {
                var durMs = (float) DecayRange.GetRangeValue(thumbMapping[ThumbType.Decay]);
                return TimeSpan.FromMilliseconds(durMs);
            }
        }

        public float SustainLevel
        {
            get
            {
                return (float) SustainRange.GetRangeValue(thumbMapping[ThumbType.Sustain]);
            }
        }

        public TimeSpan ReleaseDuration
        {
            get
            {
                var durMs = (float) ReleaseRange.GetRangeValue(thumbMapping[ThumbType.Release]);
                return TimeSpan.FromMilliseconds(durMs);
            }
        }

        public Oscillator MidiOscillator { get; private set; }

        private double ConvertFrequency(double ratio)
        {
            return Math.Pow(ratio * 10, 2) / 100;
        }

        public float OscillatorVolume { get; set; } = 0.2f;

        public bool DebugPrint { get; set; } = true;
        
        private Dictionary<ThumbType, double> thumbMapping = new Dictionary<ThumbType, double>
        {
            { ThumbType.Attack, 0.5 },
            { ThumbType.Decay, 0.5 },
            { ThumbType.CutoffLow, 0.0 },
            { ThumbType.Sustain, 0.5 },
            { ThumbType.Release, 0.5 },
            { ThumbType.CutoffHigh, 1.0 },
        };

        public void SetThumb(ThumbType thumb, double value)
        {
            thumbMapping[thumb] = value;
            updateADSR();
            updateFrequencyFilter();
        }

        public void Initialize(SoundIOSharp.SoundIODevice device, AudioFormat format)
        {
            InitSynthesizer(device, format);
            MidiOscillator = new Oscillator(Wave.CurrentState, 0, OscillatorVolume);
            Recorder = new RecordFilter(Synthesizer);

            if (DebugPrint)
            {
                Console.WriteLine($"Initialized : {device.Name}, {format.SampleRate}Hz, {Output.ActualLatency.TotalMilliseconds}ms");
            }
        }

        public void InitializeMidi(MidiFile midi, SoundIOSharp.SoundIODevice device, AudioFormat format)
        {
            InitSynthesizer(device, format);
            MidiOscillator = new Oscillator(Wave.CurrentState, 0, OscillatorVolume);
            MidiPlayer = new MidiPlayer(format.SampleRate, Synthesizer);
            MidiPlayer.OscillatorConfigs.Add(MidiOscillator);
            MidiPlayer.Open(midi);

            if (DebugPrint)
            {
                Console.WriteLine($"Initialized : {device.Name}, {format.SampleRate}Hz, {Output.ActualLatency.TotalMilliseconds}ms");
            }
        }

        private void InitSynthesizer(SoundIOSharp.SoundIODevice device, AudioFormat format)
        {
            if (Output == null)
            {
                Output = new SoundioOutput();
            }
            
            Output.DesiredLatency = TimeSpan.FromMilliseconds(100);
            Output.Initialize(device, format);
            Output.Buffer += OutputOnBuffer;

            Synthesizer = new Synthesizer(format.SampleRate, 50);
        }

        private void OutputOnBuffer(object? sender, OutputBufferEventArgs e)
        {
            if (Recorder != null)
            {
                e.Buffer = Recorder.Next(e.Size);
            }
            else
            {
                e.Buffer = MidiPlayer.Next(e.Size);
            }
        }

        public Task Run(IEventInput input, CancellationToken cancellationToken)
        {
            return Task.Run(() => RunInternal(input, cancellationToken));
        }

        private void RunInternal(IEventInput input, CancellationToken cancellationToken)
        {
            if (Output == null)
            {
                throw new Exception("SynthesizerPlayer was not initialized");
            }

            Output.Play();
            
            updateADSR();
            updateFrequencyFilter();
            
            bool exit = false;
            while(!input.Exit && !exit && !cancellationToken.IsCancellationRequested)
            {
                var result = input.Read();
                if (input.Event == null)
                {
                    continue;
                }

                switch (result)
                {
                    case EventReadResult.Failed:
                        exit = true;
                        Console.WriteLine("Failed to read next buffer");
                        continue;
                    case EventReadResult.Timeout:
                        continue;
                }

                if (DebugPrint)
                {
                    Console.WriteLine($"Event : {input.Event}");
                }
                switch (input.Event.Type)
                {
                    case ToneEventType.Key:
                        var key = (ToneKeyEvent) input.Event;
                        handleKeyEvent(key);
                        break;
                    case ToneEventType.Thumb:
                        var thumb = (ToneThumbEvent) input.Event;
                        handleThumbEvent(thumb);
                        break;
                    case ToneEventType.Filter:
                        handleFilterEvent();
                        break;
                    case ToneEventType.Wave:
                        handleWaveEvent();
                        break;
                }
            }

            Output.Buffer -= OutputOnBuffer;
            Output.Stop();
            
            if (DebugPrint)
            {
                Console.WriteLine("Output has stopped");
            }
            
            Output.Dispose();

            if (DebugPrint)
            {
                Console.WriteLine("Thread exited successfully");
            }
        }

        public void StartRecording(string path)
        {
            if (Recorder.IsRecording)
            {
                return;
            }

            var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            Recorder.StartRecording(fs, true, false);
        }

        public void StopRecording()
        {
            Recorder.StopRecording();
        }

        private void handleFilterEvent()
        {
            Filter.Next();
            if (DebugPrint)
            {
                Console.WriteLine($"Filter : {Filter.CurrentState}");
            }
            
            switch (Filter.CurrentState)
            {
                case FilterMode.Disabled:
                    Synthesizer.FrequencyFilter.Disable();
                    break;
                case FilterMode.Lowpass:
                    Synthesizer.FrequencyFilter.SetLowpassMode(CutoffLow);
                    break;
                case FilterMode.Highpass:
                    Synthesizer.FrequencyFilter.SetHighpassMode(CutoffHigh);
                    break;
                case FilterMode.Bandpass:
                    Synthesizer.FrequencyFilter.SetBandpassMode(CutoffLow, CutoffHigh);
                    break;
            }
        }

        private void handleWaveEvent()
        {
            Wave.Next();

            if (MidiOscillator != null)
            {
                MidiOscillator.Function = Wave.CurrentState;
            }
            
            if (DebugPrint)
            {
                Console.WriteLine($"Wave : {Wave.CurrentState}");
            }
        }

        private void handleThumbEvent(ToneThumbEvent e)
        {
            if (!thumbMapping.ContainsKey(e.Type))
            {
                return;
            }
            
            var val = thumbMapping[e.Type];
            switch (e.Direction)
            {
                case ThumbDirection.Clockwise:
                    val += ThumbStep;
                    break;
                case ThumbDirection.CounterClockwise:
                    val -= ThumbStep;
                    break;
            }
            val = Math.Min(1, Math.Max(0, val));

            thumbMapping[e.Type] = val;

            switch (e.Type)
            {
                case ThumbType.CutoffLow:
                case ThumbType.CutoffHigh:
                    updateFrequencyFilter();
                    break;
                case ThumbType.Attack:
                case ThumbType.Decay:
                case ThumbType.Sustain:
                case ThumbType.Release:
                    updateADSR();
                    break;
            }
        }

        private void updateADSR()
        {
            if (DebugPrint)
            {
                Console.WriteLine($"A {AttackDuration.TotalMilliseconds}ms, D {DecayDuration.TotalMilliseconds}ms, S {SustainLevel}, R {ReleaseDuration.TotalMilliseconds}ms");
            }

            if (Synthesizer == null)
            {
                return;
            }

            Synthesizer.AttackDuration = AttackDuration;
            Synthesizer.DecayDuration = DecayDuration;
            Synthesizer.Sustain = SustainLevel;
            Synthesizer.ReleaseDuration = ReleaseDuration;
        }

        private void updateFrequencyFilter()
        {
            if (DebugPrint)
            {
                Console.WriteLine($"Cutoff : Low {CutoffLow}Hz, High {CutoffHigh}Hz");
            }

            if (Synthesizer == null)
            {
                return;
            }
            
            Synthesizer.FrequencyFilter.ChangeFrequency(CutoffLow, CutoffHigh);
        }

        private void handleKeyEvent(ToneKeyEvent e)
        {
            if (DebugPrint && !e.Release) 
            {
                Console.WriteLine($"{(e.Release ? "Release" : "Attack")} : {e.Tone.Scale}, {e.Tone.Octave}");
            }
            if (e.Release)
            {
                Release(e.Tone);
            }
            else
            {
                Attack(e.Tone);
            }
        }

        public void Attack(IonianTone tone)
        {
            if (toneMap.ContainsKey(tone))
            {
                return;
            }
            
            var freq = Scale.GetFrequency(tone);
            var index = Synthesizer.Attack(new Oscillator[] { new Oscillator(Wave.CurrentState, freq, OscillatorVolume) });
            toneMap[tone] = index;
        }

        public void Release(IonianTone tone)
        {
           if (toneMap.TryGetValue(tone, out var index))
            {
                toneMap.Remove(tone);
                Synthesizer.Release(index);
            }
        }
    }
}
