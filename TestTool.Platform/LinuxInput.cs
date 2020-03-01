using System;
using KSynthesizer.Linux;
using KSynthesizer.Linux.InputEventEnum;

namespace TestTool.Platform
{
    public class LinuxInput : IConsoleInput
    {
        private readonly KeyboardInput KeyboardInput;
        
        public event EventHandler<InputEventArgs> Attack;
        
        public event EventHandler Release;
        
        public event EventHandler Exit;

        public void Listen()
        {
            KeyboardInput.Listen();
        }

        public void Stop()
        {
            KeyboardInput.Stop();
        }

        public LinuxInput()
        {
            var keyboards = KeyboardInput.Keyboards;
            if (keyboards.Length == 0)
            {
                Console.WriteLine("There is no keyboard available");
                return;
            }

            string keyboard = null;
            if (keyboards.Length == 1)
            {
                keyboard = keyboards[0];
            }
            else
            {
                for (int i = 0; keyboards.Length > i; i++)
                {
                    Console.WriteLine($"[{i}] {keyboards[i]}");
                }
                
                while (keyboard == null)
                {
                    Console.Write("Device Index > ");
                    var num = Console.ReadLine();
                    if (int.TryParse(num, out var i) && i >= 0 && i < keyboards.Length)
                    {
                        keyboard = keyboards[i];
                        break;
                    }
                }
            }
            
            KeyboardInput = new KeyboardInput(keyboard);
            KeyboardInput.Keys.Add(InputKeyCodes.KEY_0);
            KeyboardInput.Keys.Add(InputKeyCodes.KEY_1);
            KeyboardInput.Keys.Add(InputKeyCodes.KEY_2);
            KeyboardInput.Keys.Add(InputKeyCodes.KEY_3);
            KeyboardInput.Keys.Add(InputKeyCodes.KEY_4);
            KeyboardInput.Keys.Add(InputKeyCodes.KEY_5);
            KeyboardInput.Keys.Add(InputKeyCodes.KEY_6);
            KeyboardInput.Keys.Add(InputKeyCodes.KEY_7);
            KeyboardInput.Keys.Add(InputKeyCodes.KEY_8);
            KeyboardInput.Keys.Add(InputKeyCodes.KEY_9);
            KeyboardInput.Keys.Add(InputKeyCodes.KEY_Q);
            KeyboardInput.Attack += KeyboardInputOnAttack;
            KeyboardInput.Release += KeyboardInputOnRelease;
            KeyboardInput.ErrorHandler = ErrorHandler;
        }

        private static void ErrorHandler(Exception obj)
        {
            Console.WriteLine("Keyboard Input Error!");
            Console.WriteLine(obj);
        }
        
        private void KeyboardInputOnRelease(object? sender, InputEventArgs<InputKeyCodes> e)
        {
            Release?.Invoke(this, EventArgs.Empty);
        }

        private void KeyboardInputOnAttack(object? sender, InputEventArgs<InputKeyCodes> e)
        {
            float freq = 0;
            switch (e.Input)
            {
                case InputKeyCodes.KEY_0:
                    freq = 100;
                    break;
                case InputKeyCodes.KEY_1:
                    freq = 200;
                    break;
                case InputKeyCodes.KEY_2:
                    freq = 300;
                    break;
                case InputKeyCodes.KEY_3:
                    freq = 400;
                    break;
                case InputKeyCodes.KEY_4:
                    freq = 500;
                    break;
                case InputKeyCodes.KEY_5:
                    freq = 600;
                    break;
                case InputKeyCodes.KEY_6:
                    freq = 700;
                    break;
                case InputKeyCodes.KEY_7:
                    freq = 800;
                    break;
                case InputKeyCodes.KEY_8:
                    freq = 900;
                    break;
                case InputKeyCodes.KEY_9:
                    freq = 1000;
                    break;
                case InputKeyCodes.KEY_Q:
                    Exit?.Invoke(this, EventArgs.Empty);
                    return;
                default:
                    Console.WriteLine($"Unknown Key Input : {e.Input}");
                    return;
            }
            
            Attack?.Invoke(this, new InputEventArgs()
            {
                Frequency = freq,
            });
        }
    }
}