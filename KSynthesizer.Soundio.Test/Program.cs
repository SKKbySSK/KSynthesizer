using System;
using System.IO;
using KSynthesizer.Filters;
using KSynthesizer.Sources;

namespace KSynthesizer.Soundio.Test
{
    class Program
    {
        private static SoundioOutput Output { get; set; }
        
        private static PeriodicFunctionsSource Source { get; set; }
        
        private static  RecordFilter RecordFilter { get; set; }

        private static bool Playing { get; set; } = true;

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
            output.Initialize(output.Devices[outputIndex], new AudioFormat(48000, 1, 32));
            Output = output;
            Console.WriteLine("Output device initialized");
            Console.WriteLine($"Actual Latency : {output.ActualLatency.TotalMilliseconds}ms");
            
            Source = new PeriodicFunctionsSource(output.Format.SampleRate)
            {
                Function = FunctionType.Sin,
                Volume = 1,
            };
            Source.SetFrequency(440);
            RecordFilter = new RecordFilter(Source);
            
            Output.Tick += OutputOnTick;
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

        private static void OutputOnTick(object? sender, EventArgs e)
        {
            if (Playing)
            {
                Output.Write(RecordFilter.Next(Output.DesiredFillSize));
            }
            else
            {
                Output.Write(new float[Output.DesiredFillSize]);
            }
        }
    }
}
