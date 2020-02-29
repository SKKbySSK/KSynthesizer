using System;
using System.IO;
using System.Runtime.InteropServices;
using KSynthesizer;
using KSynthesizer.Linux;
using KSynthesizer.Linux.InputEventEnum;
using KSynthesizer.Sources;

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
