using ConsoleAppFramework;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using KSynthesizer.Filters;
using KSynthesizer.Soundio;
using KSynthesizer.Sources;
using Melanchall.DryWetMidi.Core;

namespace KSynthesizer.Cli
{
    class Program : ConsoleAppBase
    {
        static async Task Main(string[] args)
        {
            // target T as ConsoleAppBase.
            await Host.CreateDefaultBuilder().RunConsoleAppFrameworkAsync<Program>(args);
        }
        
        static string[] ParseArguments(string commandLine)
        {
            char[] parmChars = commandLine.ToCharArray();
            bool inQuote = false;
            for (int index = 0; index < parmChars.Length; index++)
            {
                if (parmChars[index] == '"')
                    inQuote = !inQuote;
                if (!inQuote && parmChars[index] == ' ')
                    parmChars[index] = '\n';
            }
            return (new string(parmChars)).Split('\n');
        }

        private FunctionType StringToFunctionType(string function, FunctionType defaultValue = FunctionType.Sin)
        {
            switch (function.ToLower())
            {
                case "sin":
                    return FunctionType.Sin;
                case "rect":
                    return FunctionType.Rect;
                case "sawtooth":
                    return FunctionType.Sawtooth;
                case "triangle":
                    return FunctionType.Triangle;
            }

            return defaultValue;
        }

        public async Task Run([Option("s", "Serial port number")] int serial,
            [Option("d", "Output device index")] int? device = null,
            [Option("r", "Sample rate")] int sampleRate = 48000)
        {
            var player = new SynthesizerPlayer();
            var dev = player.Output.DefaultDevice;
            if (device != null)
            {
                dev = player.Output.Devices[device.Value];
            }
            
            var input = new SerialEventInput();
            input.SetPortName(input.SerialPortNames[serial]);

            var format = new AudioFormat(sampleRate, 1, 32);
            player.Initialize(dev, format);

            var source = new CancellationTokenSource();
            var runnerTask = player.Run(input, source.Token);

            bool exit = false;
            string[] cmd;
            while (!exit)
            {
                Console.Write(" > ");
                cmd = ParseArguments(Console.ReadLine());

                if (cmd.Length == 0)
                {
                    continue;
                }
                
                switch (cmd[0])
                {
                    case "":
                        break;
                    case "exit":
                        source.Cancel();
                        await runnerTask;
                        exit = true;
                        break;
                    case "record":
                        switch (cmd[1])
                        {
                            case "start":
                                if (cmd.Length <= 3)
                                {
                                    Console.WriteLine("Path was not specified");
                                    continue;
                                }
                                try
                                {
                                    player.StartRecording(cmd[1]);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex);
                                    Console.WriteLine($"Failed to create {cmd[1]} for writing");
                                }
                                break;
                            case "stop":
                                player.StopRecording();
                                break;
                        }
                        break;
                    default:
                        Console.WriteLine($"Unknown command");
                        break;
                }
            }
            
            input.Dispose();
        }


        [Command("midi")]
        public async Task Midi([Option("m", "Midi file")] string midi,
            [Option("f", "Function type")] string function = "sin",
            [Option("s", "Serial port number")] int? serial = null,
            [Option("d", "Output device index")] int? device = null,
            double attack = 0.1,
            double decay = 0.1,
            double sustain = 0,
            double release = 0.2)
        {
            var player = new SynthesizerPlayer();
            var dev = player.Output.DefaultDevice;
            if (device != null)
            {
                dev = player.Output.Devices[device.Value];
            }

            var file = MidiFile.Read(midi);

            IEventInput input;

            if (serial != null)
            {
               var serialInput = new SerialEventInput();
               serialInput.SetPortName(serialInput.SerialPortNames[serial.Value]);
               input = serialInput;
            }
            else
            {
                input = new ManualEventInput();
            }

            var format = new AudioFormat(48000, 1, 32);
            player.SetThumb(ThumbType.Attack, attack);
            player.SetThumb(ThumbType.Decay, decay);
            player.SetThumb(ThumbType.Sustain, sustain);
            player.SetThumb(ThumbType.Release, release);
            player.InitializeMidi(file, dev, format);

            var source = new CancellationTokenSource();
            var runnerTask = player.Run(input, source.Token);

            bool exit = false;
            string line;
            while (!exit)
            {
                Console.Write(" > ");
                line = Console.ReadLine();
                switch (line)
                {
                    case "":
                        break;
                    case "exit":
                        source.Cancel();
                        await runnerTask;
                        exit = true;
                        break;
                    default:
                        if (input is ManualEventInput manual)
                        {
                            switch (line)
                            {
                                case "filter":
                                    manual.NextEvent = new ToneFilterEvent();
                                    break;
                                case "wave":
                                    manual.NextEvent = new ToneWaveEvent();
                                    break;
                            }
                        }
                        Console.WriteLine($"Unknown command");
                        break;
                }
            }

            if (input is IDisposable disp)
            {
                disp.Dispose();
            }
        }
        
        [Command("manual")]
        public async Task Manual([Option("d", "Output device index")] int? device = null,
            [Option("r", "Sample rate")] int sampleRate = 48000)
        {
            var player = new SynthesizerPlayer();
            var dev = player.Output.DefaultDevice;
            if (device != null)
            {
                dev = player.Output.Devices[device.Value];
            }
            
            var input = new ManualEventInput();
            var format = new AudioFormat(sampleRate, 1, 32);
            player.Initialize(dev, format);

            var source = new CancellationTokenSource();
            var runnerTask = player.Run(input, source.Token);
            
            var tone = new IonianTone()
            {
                Scale = IonianScale.A,
                Octave = 4,
                Sharp = false,
            };

            bool exit = false;
            string line;
            while (!exit)
            {
                Console.Write(" > ");
                line = Console.ReadLine();
                switch (line)
                {
                    case "":
                        break;
                    case "attack":
                        input.NextEvent = new ToneKeyEvent(tone, false);
                        break;
                    case "release":
                        input.NextEvent = new ToneKeyEvent(tone, true);
                        break;
                    case "filter":
                        input.NextEvent = new ToneFilterEvent();
                        break;
                    case "wave":
                        input.NextEvent = new ToneWaveEvent();
                        break;
                    case "1":
                        input.NextEvent = new ToneThumbEvent(ThumbType.CutoffLow, ThumbDirection.CounterClockwise);
                        break;
                    case "2":
                        input.NextEvent = new ToneThumbEvent(ThumbType.CutoffLow, ThumbDirection.Clockwise);
                        break;
                    case "3":
                        input.NextEvent = new ToneThumbEvent(ThumbType.CutoffHigh, ThumbDirection.CounterClockwise);
                        break;
                    case "4":
                        input.NextEvent = new ToneThumbEvent(ThumbType.CutoffHigh, ThumbDirection.Clockwise);
                        break;
                    case "5":
                        input.NextEvent = new ToneThumbEvent(ThumbType.Sustain, ThumbDirection.CounterClockwise);
                        break;
                    case "6":
                        input.NextEvent = new ToneThumbEvent(ThumbType.Sustain, ThumbDirection.Clockwise);
                        break;
                    case "exit":
                        source.Cancel();
                        await runnerTask;
                        exit = true;
                        break;
                    default:
                        Console.WriteLine($"Unknown command");
                        break;
                }
            }
        }
        
        [Command("speaker-test")]
        public async Task SpeakerTest([Option("d", "Output device index")] int? device = null,
            [Option("r", "Sample rate")] int sampleRate = 48000)
        {
            var player = new SynthesizerPlayer();
            var dev = player.Output.DefaultDevice;
            if (device != null)
            {
                dev = player.Output.Devices[device.Value];
            }
            
            var format = new AudioFormat(sampleRate, 1, 32);
            player.Initialize(dev, format);
            
            var source = new CancellationTokenSource();
            await player.Run(new TestEventInput(), source.Token);
        }

        [Command("devices")]
        public void Devices()
        {
            var output = new SoundioOutput();
            for (var i = 0; i < output.Devices.Count; i++)
            {
                Console.WriteLine($"[{i}] {output.Devices[i].Name}");
            }
            output.Dispose();
        }

        [Command("serial")]
        public void Serial()
        {
            var input = new SerialEventInput();
            for (var i = 0; i < input.SerialPortNames.Length; i++)
            {
                Console.WriteLine($"[{i}] {input.SerialPortNames[i]}");
            }
        }
    }
}
