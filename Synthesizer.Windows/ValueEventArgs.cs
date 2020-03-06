using System;
using System.Collections.Generic;
using System.Text;

namespace Synthesizer.Windows
{
    public class ValueEventArgs<T>
    {
        public ValueEventArgs(T value)
        {
            Value = value;
        }

        public T Value { get; }
    }

    public delegate void ValueEventHandler<T>(object sender, ValueEventArgs<T> e);
}
