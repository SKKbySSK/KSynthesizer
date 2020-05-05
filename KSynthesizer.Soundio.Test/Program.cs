using System;
using KSynthesizer.Sources;

namespace KSynthesizer.Soundio.Test
{
    class Program
    {
        private static SoundioOutput Output { get; set; }
        
        private static PeriodicFunctionsSource Source { get; set; }
        
        private static Clock Clock { get; set; }

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

            var clock = new Clock(output.ActualLatency);
            clock.Tick += ClockOnTick;
            Clock = clock;
            clock.Start();
            Output.Play();
            Console.WriteLine("Player and Clock started");

            bool exit = false;
            while(!exit)
            {
                var key = Console.ReadKey();
                Console.WriteLine();
                switch (key.Key)
                {
                    case ConsoleKey.P:
                        Playing = !Playing;
                        Console.WriteLine(Playing ? "Playing" : "Paused");
                        break;
                    case ConsoleKey.Escape:
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Unknown Key");
                        break;
                }
            }
            
            clock.Stop();
            output.Dispose();
            Console.WriteLine("Exited");
        }

        private static void ClockOnTick(object? sender, EventArgs e)
        {
            var addInterval = TimeSpan.FromMilliseconds(10);
            var elapsed = (Clock.Interval + addInterval).TotalSeconds;
            var size = elapsed * Output.Format.Channels * Output.Format.SampleRate *
                Output.Format.BitDepth / 8;

            if (Playing)
            {
                Output.Write(Source.Next((int)Math.Round(size)));
            }
            else
            {
                Output.Write(new float[(int)Math.Round(size)]);
            }
        }
    }
}
