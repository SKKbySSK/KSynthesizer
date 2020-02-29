﻿using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using KSynthesizer;
using KSynthesizer.Linux;
using KSynthesizer.Linux.InputEventEnum;
using KSynthesizer.Sources;
using SoundIOSharp;

namespace TestTool.Platform
{
    class Program
    {
        private static KeyboardInput KeyboardInput { get; } = new KeyboardInput(KeyboardInput.Keyboards[0]);

        private static EnvelopeGenerator EnvelopeGenerator;
        
        private static PeriodicFunctionsSource Source { get; } = new PeriodicFunctionsSource(44100);

        private static AudioPlayer Player;
        
        static unsafe void Main(string[] args)
        {
            KeyboardInput.Keys.Add(InputKeyCodes.KEY_0);
            KeyboardInput.Keys.Add(InputKeyCodes.KEY_1);
            KeyboardInput.Attack += KeyboardInputOnAttack;
            KeyboardInput.Release += KeyboardInputOnRelease;

            Source.Function = FunctionType.Sin;
            Source.SetFrequency(400);
            EnvelopeGenerator = new EnvelopeGenerator(Source);
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
                var num = Console.ReadLine();
                if (int.TryParse(num, out var i) && i >= 0 && i < Player.Devices.Count)
                {
                    device = Player.Devices[i];
                    break;
                }
            }
            
            Player.Init(device);
            KeyboardInput.Listen();
            Player.Start();
            while (true)
            {
                var c = (char)Console.Read();
                if (c == 'q')
                {
                    break;
                }
            }
            Player.Dispose();
		}

        private static void KeyboardInputOnRelease(object? sender, InputEventArgs<InputKeyCodes> e)
        {
            EnvelopeGenerator.Release();
        }

        private static void KeyboardInputOnAttack(object? sender, InputEventArgs<InputKeyCodes> e)
        {
            EnvelopeGenerator.Attack();
        }
    }
}
