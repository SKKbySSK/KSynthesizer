using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using KSynthesizer.Linux.InputEventEnum;

namespace KSynthesizer.Linux
{
    public class KeyboardInput : ISynthesizerInput<InputKeyCodes>
    {

        public static String[] Keyboards
        {
            get => Directory.GetFiles("/dev/input/by-path/", "*-kbd");
        }

        public KeyboardInput(string keyboard)
        {
            Keyboard = keyboard;
        }
        
        private bool listening = false;

        public event EventHandler<InputEventArgs<InputKeyCodes>> Attack;
        
        public event EventHandler<InputEventArgs<InputKeyCodes>> Release;

        public List<InputKeyCodes> Keys { get; } = new List<InputKeyCodes>();
        
        public Action<Exception> ErrorHandler { get; set; }
        
        public string Keyboard { get; }

        public unsafe void Listen()
        {
            if (listening)
            {
                return;
            }
            
            listening = true;
            
            Task.Run(() =>
            {
                try
                {
                    using (var stream = new FileStream(Keyboard, FileMode.Open, FileAccess.Read))
                    {
                        byte[] buffer = new byte[Environment.Is64BitProcess ? sizeof(NativeInputEvent64) : sizeof(NativeInputEvent32)];
                        while (listening)
                        {
                            stream.Read(buffer, 0, buffer.Length);
                            HandleInputEvent(ToInputEvent(buffer));
                        }
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    ErrorHandler?.Invoke(new InputException("Program must be executed by super user", ex));
                }
                catch (Exception ex)
                {
                    ErrorHandler?.Invoke(new InputException("Unknown Error Occured", ex));
                }
                listening = false;
            });
        }

        private unsafe InputEvent ToInputEvent(byte[] buffer)
        {
            fixed (byte* ptr = buffer)
            {
                if (Environment.Is64BitProcess)
                {
                    var ev = (NativeInputEvent64)Marshal.PtrToStructure((IntPtr)ptr, typeof(NativeInputEvent64));
                    return new InputEvent(ev);
                }
                else
                {
                    var ev = (NativeInputEvent32)Marshal.PtrToStructure((IntPtr)ptr, typeof(NativeInputEvent32));
                    return new InputEvent(ev);
                }
            }
        }
        
        private void HandleInputEvent(InputEvent input)
        {
            if (input.Type != InputType.EV_KEY)
            {
                return;
            }
            
            var key = (InputKeyCodes)input.Code;
            if (!Keys.Contains(key))
            {
                return;
            }
            
            if (input.Value == 1) // Key Down
            {
                Attack?.Invoke(this, new InputEventArgs<InputKeyCodes>(key));
            }
            else if (input.Value == 0) // Key Up
            {
                Release?.Invoke(this, new InputEventArgs<InputKeyCodes>(key));
            }
        }

        public void Stop()
        {
            listening = false;
        }
    }
}