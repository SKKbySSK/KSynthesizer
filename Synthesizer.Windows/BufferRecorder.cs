using KSynthesizer;
using KSynthesizer.Filters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Synthesizer.Windows
{
    public class BufferRecorder
    {
        public BufferRecorder(AudioFormat format)
        {
            Format = format;
        }

        public AudioFormat Format { get; }

        private List<float> lastBuffer = new List<float>();
        public float[] LastBuffer => lastBuffer.ToArray();

        public int BufferLength { get; set; } = 1024;

        public bool IsRecording { get; private set; } = false;

        private BinaryWriter binaryWriter = new BinaryWriter(new FileStream("test.raw", FileMode.Create));

        public void Record()
        {
            IsRecording = true;
        }

        public void Stop()
        {
            IsRecording = false;
            lock(this)
            {
                lastBuffer.Clear();
            }
        }

        public void Append(float[] buffer)
        {
            if (!IsRecording)
            {
                return;
            }

            lastBuffer.AddRange(buffer);
            foreach(var b in buffer)
            {
                binaryWriter.Write(b);
            }
            binaryWriter.Flush();

            if (lastBuffer.Count > BufferLength)
            {
                lastBuffer.RemoveRange(0, lastBuffer.Count - BufferLength);
            }
        }
    }
}
