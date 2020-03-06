using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using KSynthesizer;
using KSynthesizer.Linux;
using KSynthesizer.Linux.InputEventEnum;
using KSynthesizer.Sources;
using SoundIOSharp;

namespace TestTool.Platform
{
    class Program
    {
        private static VolumeEnvelope EnvelopeGenerator;
        
        private static PeriodicFunctionsSource Source { get; } = new PeriodicFunctionsSource(44100);

        private static AudioPlayer Player;

        private static IConsoleInput Input;

        private static bool exit = false;
        
        static void Main(string[] args)
        {
            
            Source.Function = FunctionType.Sin;
            EnvelopeGenerator = new VolumeEnvelope(Source);
            EnvelopeGenerator.Released += EnvelopeGeneratorOnReleased;
            Player = new AudioPlayer(EnvelopeGenerator);

            foreach (var (i, dev) in Player.Devices.Select((dev, i) => (i, dev)))
            {
                if (i == Player.DefaultDeviceIndex)
                {
                    Console.WriteLine($"[{i}][Default] {dev.Name}");
                }
                else
                {
                    Console.WriteLine($"[{i}] {dev.Name}");
                }
            }

            SoundIODevice device = null;
            while (device == null)
            {
                Console.Write("Device Index > ");
                var num = Console.ReadLine();
                if (int.TryParse(num, out var i) && i >= 0 && i < Player.Devices.Count)
                {
                    device = Player.Devices[i];
                    break;
                }
            }

            try
            {
                Player.Init(device);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Player.Dispose();
                return;
            }
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Input = new LinuxInput();
            }
            else
            {
                Input = new CommonInput();
            }
            
            Input.Attack += InputOnAttack;
            Input.Release += InputOnRelease;
            Input.Exit += InputOnExit;
            Input.Listen();

            while (!exit)
            {
                Thread.Sleep(100);
            }
            Player.Dispose();
            Input.Stop();
        }

        private static void InputOnExit(object sender, EventArgs e)
        {
            exit = true;
        }

        private static void InputOnRelease(object sender, EventArgs e)
        {
            Console.WriteLine($"Release");
            EnvelopeGenerator.Release();
        }

        private static void InputOnAttack(object sender, InputEventArgs e)
        {
            Console.WriteLine($"Attack : {e.Frequency}Hz");
            Source.SetFrequency(e.Frequency);
            EnvelopeGenerator.Attack();
            Player.Play();
        }

        private static void EnvelopeGeneratorOnReleased(object sender, EventArgs e)
        {
            Player.Pause();
        }
    }
}
