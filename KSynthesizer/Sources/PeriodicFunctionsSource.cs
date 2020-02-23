using System;
using System.Collections.Generic;
using System.Text;

namespace KSynthesizer.Sources
{
    public enum FunctionType
    {
        DC,
        Sin,
        Rect,
        Sawtooth,
        Triangle,
    }

    public class PeriodicFunctionsSource : PeriodicSourceBase
    {
        private Dictionary<FunctionType, PeriodicSourceBase> sources = new Dictionary<FunctionType, PeriodicSourceBase>();

        public PeriodicFunctionsSource(int sampleRate)
        {
            foreach (var func in Enum.GetValues(typeof(FunctionType)))
            {
                PeriodicSourceBase source = null;
                switch ((FunctionType)func)
                {
                    case FunctionType.DC:
                        source = new DcSource(sampleRate);
                        break;
                    case FunctionType.Sin:
                        source = new SinSource(sampleRate);
                        break;
                    case FunctionType.Rect:
                        source = new RectSource(sampleRate);
                        break;
                    case FunctionType.Sawtooth:
                        source = new SawtoothSource(sampleRate);
                        break;
                    case FunctionType.Triangle:
                        source = new TriangleSource(sampleRate);
                        break;
                    default:
                        Console.WriteLine("Unknown type : " + func);
                        break;
                }
                sources[(FunctionType)func] = source;
            }

            Period = 1000;
            Format = new AudioFormat(sampleRate, 1, 32);
        }

        public override float Period
        {
            get => base.Period;
            set
            {
                base.Period = value;

                foreach (var pair in sources)
                {
                    if (pair.Value != null)
                    {
                        pair.Value.Period = value;
                    }
                }
            }
        }

        public float DcValue
        {
            get => GetSource<DcSource>().Value;
            set => GetSource<DcSource>().Value = value;
        }

        public float RectDuty
        {
            get => GetSource<RectSource>().Duty;
            set => GetSource<RectSource>().Duty = value;
        }

        public FunctionType Function { get; set; } = FunctionType.DC;

        public PeriodicSourceBase Source
        {
            get => sources[Function];
        }

        public override float[] Next(int size)
        {
            return sources[Function]?.Next(size) ?? new float[size];
        }

        protected override void GenerateNextBufferForPeriod(float[] buffer)
        {
        }

        private T GetSource<T>() where T : PeriodicSourceBase
        {
            foreach (var pair in sources)
            {
                if (pair.Value is T)
                {
                    return (T)pair.Value;
                }
            }

            return null;
        }
    }
}
