using System;
using System.IO;
using KSynthesizer.Filters;
using KSynthesizer.Midi;
using KSynthesizer.Sources;
using Melanchall.DryWetMidi.Core;

namespace KSynthesizer.Soundio.Test
{
    class Program
    {
        private static SoundioOutput Output { get; set; }
        
        private static PeriodicFunctionsSource Source { get; set; }
        
        private static  RecordFilter RecordFilter { get; set; }

        private static bool Playing { get; set; } = true;

        private static int Frequency { get; set; } = 440;

        static void Main(string[] args)
        {
            var output = new SoundioOutput();
            for (var i = 0; i < output.Devices.Count; i++)
            {
                Console.WriteLine($"[{i}] {output.Devices[i].Name}");
            }

            Console.Write("Device Index > ");
            var outputIndex = int.Parse(Console.ReadLine());
            
            Console.Write("Latency(ms) > ");
            var intervalMs = Double.Parse(Console.ReadLine());

            output.DesiredLatency = TimeSpan.FromMilliseconds(intervalMs);
            output.Initialize(output.Devices[outputIndex], new AudioFormat(48000, 1, 16));
            Output = output;
            Console.WriteLine("Output device initialized");
            Console.WriteLine($"Actual Latency : {output.ActualLatency.TotalMilliseconds}ms");
            
            Source = new PeriodicFunctionsSource(output.Format.SampleRate)
            {
                Function = FunctionType.Sin,
                Volume = 1,
            };

            var midi = new MidiPlayer(output.Format.SampleRate, new Synthesizer(output.Format.SampleRate));
            midi.OscillatorConfigs.Add(new Oscillator() { Function = KSynthesizer.Sources.FunctionType.Sin, Volume = 0.3f });
            midi.Open(MidiFile.Read("sample.mid"));
            
            Source.SetFrequency(Frequency);
            RecordFilter = new RecordFilter(midi);
            
            Output.Buffer += OutputOnBuffer;
            Output.Play();
            Console.WriteLine("Player started");

            bool exit = false;
            while(!exit)
            {
                var key = Console.ReadKey();
                Console.WriteLine();
                switch (key.Key)
                {
                    case ConsoleKey.P:
                        Playing = !Playing;
                        Console.WriteLine(Playing ? "Resumed" : "Paused");
                        break;
                    case ConsoleKey.R:
                        if (RecordFilter.IsRecording)
                        {
                            RecordFilter.StopRecording();
                        }
                        else
                        {
                            var now = DateTime.Now;
                            var name = $"{now.Ticks}.wav";
                            RecordFilter.StartRecording(new FileStream(name, FileMode.Create, FileAccess.Write), true, false);
                            Console.WriteLine("Output : " + name);
                        }
                        Console.WriteLine(RecordFilter.IsRecording ? "Recording Started" : "Recording Stopped");
                        break;
                    case ConsoleKey.UpArrow:
                        Frequency += 100;
                        Source.SetFrequency(Frequency);
                        Console.WriteLine($"Frequency : {Frequency}Hz");
                        break;
                    case ConsoleKey.DownArrow:
                        Frequency -= 100;
                        if (Frequency <= 300)
                        {
                            Frequency = 300;
                        }
                        Source.SetFrequency(Frequency);
                        Console.WriteLine($"Frequency : {Frequency}Hz");
                        break;
                    case ConsoleKey.Escape:
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Unknown Key");
                        break;
                }
            }
            
            output.Dispose();
            Console.WriteLine("Exited");
        }

        private static void OutputOnBuffer(object? sender, OutputBufferEventArgs e)
        {
            if (Playing)
            {
                e.Buffer = RecordFilter.Next(e.Size);
            }
            else
            {
                e.Buffer = new float[e.Size];
            }
        }
    }
}
