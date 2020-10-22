using System.Dynamic;

namespace KSynthesizer.Cli
{
    enum ToneEventType
    {
        Key = 1,
        Thumb = 2,
        Filter = 3,
        Wave = 4,
    }

    enum ThumbDirection
    {
        Clockwise,
        CounterClockwise
    }

    enum ThumbType
    {
        Attack = 0,
        Decay = 1,
        CutoffLow = 2,
        Sustain = 3,
        Release = 4,
        CutoffHigh = 5,
    }

    enum FilterMode
    {
        Disabled = 0,
        Lowpass = 1,
        Highpass = 2,
        Bandpass = 3,
    }

    class ToneKeyEvent : ToneEvent
    {
        public ToneKeyEvent(IonianTone tone, bool release) : base(ToneEventType.Key)
        {
            Tone = tone;
            Release = release;
        }

        public IonianTone Tone { get; }
        
        public bool Release { get; }

        public static int DataLength { get; } = 4;

        public static ToneKeyEvent ParseEvent(byte[] data, int semitones)
        {
            var scale = (IonianScale) CharToInt(data[0]);
            var sharp = ByteToBool(data[1]);
            var octave = CharToInt(data[2]);
            var attack = ByteToBool(data[3]);
            return new ToneKeyEvent(new IonianTone()
            {
                Scale = scale,
                Octave = octave,
                Sharp = sharp,
                Semitones = semitones,
            }, !attack);
        }

        private static bool ByteToBool(byte value)
        {
            return CharToInt(value) != 0;
        }

        private static int CharToInt(byte value)
        {
            var typeChar = (char) value;
            return (int)char.GetNumericValue(typeChar);
        }
    }

    class ToneThumbEvent : ToneEvent
    {
        public ToneThumbEvent(ThumbType type, ThumbDirection direction) : base(ToneEventType.Thumb)
        {
            Type = type;
            Direction = direction;
        }
        
        public ThumbType Type { get; }
        
        public ThumbDirection Direction { get; }

        public static int DataLength { get; } = 2;

        public static ToneThumbEvent ParseEvent(byte[] data)
        {
            var type = (ThumbType) CharToInt(data[0]);
            var dir = (ThumbDirection) CharToInt(data[1]);
            var ev = new ToneThumbEvent(type, dir);
            return ev;
        }
        
        private static int CharToInt(byte value)
        {
            var typeChar = (char) value;
            return (int)char.GetNumericValue(typeChar);
        }
    }

    class ToneFilterEvent : ToneEvent
    {
        public ToneFilterEvent() : base(ToneEventType.Filter)
        {
        }
    }

    class ToneWaveEvent : ToneEvent
    {
        public ToneWaveEvent() : base(ToneEventType.Wave)
        {
        }
    }

    abstract class ToneEvent
    {
        protected ToneEvent(ToneEventType type)
        {
            Type = type;
        }

        public ToneEventType Type { get; }

        public override string ToString()
        {
            return $"Type {Type}";
        }
    }
}