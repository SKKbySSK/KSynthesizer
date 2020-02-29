using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using KSynthesizer.Linux.InputEventEnum;

namespace KSynthesizer.Linux
{
    [StructLayout(LayoutKind.Sequential)]
    struct NativeTimeval
    {
        public long tv_sec;
        public long tv_usec;
    }
	
    [StructLayout(LayoutKind.Sequential)]
    struct NativeInputEvent
    {
        public NativeTimeval Time;
        public ushort Type;
        public ushort Code;
        public int Value;
    }

    public struct InputEvent
    {
        internal InputEvent(NativeInputEvent native)
        {            
            var dtDateTime = new DateTime(1970,1,1,0,0,0,0,System.DateTimeKind.Utc);
            Time = dtDateTime.AddSeconds(native.Time.tv_sec).ToLocalTime();
            Type = (InputType) native.Type;
            Code = native.Code;
            Value = native.Value;
        }
        
        public DateTime Time;
        public InputType Type;
        public ushort Code;
        public int Value;
    }

    
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
        
        public string Keyboard { get; }

        public void Listen()
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
                        byte[] buffer = new byte[24];
                        while (listening)
                        {
                            stream.Read(buffer, 0, buffer.Length);
                            HandleInputEvent(ToInputEvent(buffer));
                        }
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    throw new InputException("Program must be executed by super user", ex);
                }

                listening = false;
            });
        }

        private unsafe InputEvent ToInputEvent(byte[] buffer)
        {
            fixed (byte* ptr = buffer)
            {
                var ev = (NativeInputEvent)Marshal.PtrToStructure((IntPtr)ptr, typeof(NativeInputEvent));
                return new InputEvent(ev);
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