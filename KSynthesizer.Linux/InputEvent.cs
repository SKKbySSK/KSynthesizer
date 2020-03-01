using System.Runtime.InteropServices;
using KSynthesizer.Linux.InputEventEnum;

namespace KSynthesizer.Linux
{
    [StructLayout(LayoutKind.Sequential)]
    struct NativeTimeval64
    {
        public long tv_sec;
        public long tv_usec;
    }
	
    [StructLayout(LayoutKind.Sequential)]
    struct NativeInputEvent64
    {
        public NativeTimeval64 Time;
        public ushort Type;
        public ushort Code;
        public int Value;
    }
    
    [StructLayout(LayoutKind.Sequential)]
    struct NativeTimeval32
    {
        public int tv_sec;
        public int tv_usec;
    }
	
    [StructLayout(LayoutKind.Sequential)]
    struct NativeInputEvent32
    {
        public NativeTimeval32 Time;
        public ushort Type;
        public ushort Code;
        public int Value;
    }

    public struct InputEvent
    {
        internal InputEvent(NativeInputEvent64 native)
        {
            Type = (InputType) native.Type;
            Code = native.Code;
            Value = native.Value;
        }
        
        internal InputEvent(NativeInputEvent32 native)
        {
            Type = (InputType) native.Type;
            Code = native.Code;
            Value = native.Value;
        }
        
        public InputType Type;
        public ushort Code;
        public int Value;
    }

}