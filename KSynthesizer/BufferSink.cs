using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KSynthesizer
{
    public class BufferSink<T>
    {
        private bool reachLast = false;

        public BufferSink(int size)
        {
            Size = size;
        }

        public int Size { get; set; }

        private List<Memory<T>> Buffers { get; } = new List<Memory<T>>();

        public bool IsFilled => reachLast || Buffers.Sum(b => b.Length) >= Size;

        public event EventHandler Filled;

        public void Push(T[] buffer, bool isLastBuffer)
        {
            Buffers.Add(buffer);
            reachLast = isLastBuffer;

            if (IsFilled)
            {
                Filled?.Invoke(this, EventArgs.Empty);
            }
        }

        public void PushCopied(T[] buffer, int offset, int count, bool isLastBuffer)
        {
            T[] copied = new T[buffer.Length];
            Array.Copy(buffer, offset, copied, 0, count);

            Push(copied, isLastBuffer);
        }

        public T[] Pop(int size)
        {
            int len = Math.Min(size, Buffers.Sum(b => b.Length));

            int read = 0;
            Memory<T>? buffer = null;
            T[] data = new T[len];
            for (int i = 0; len > i; i++)
            {
                if (buffer == null)
                {
                    buffer = Buffers[0];
                    Buffers.RemoveAt(0);
                }

                if (i - read < buffer.Value.Length)
                {
                    data[i] = buffer.Value.Span[i - read];
                }
                else
                {
                    read += buffer.Value.Length;
                    buffer = null;
                    i--;
                }
            }

            if (buffer != null)
            {
                int ind = buffer.Value.Length - (len - read);

                if (ind > 0)
                {
                    Buffers.Insert(0, buffer.Value.Slice(ind));
                }
            }

            return data;
        }

        public T[] Pop()
        {
            return Pop(Size);
        }

        public void Reset()
        {
            Buffers.Clear();
            reachLast = false;
        }
    }
}
