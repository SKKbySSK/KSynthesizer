using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace KSynthesizer.Filters
{
    [StructLayout(LayoutKind.Explicit)]
    public struct UnionArray
    {
        [FieldOffset(0)]
        public byte[] Bytes;

        [FieldOffset(0)]
        public short[] Shorts;
    }
    
    public class RecordFilter : AudioFilterBase
    {
        private Thread thread;
        private readonly UnionArray buffer;
        private readonly RingBuffer<float> ringBuffer;
        private readonly object lockObj = new object();

        public RecordFilter(IAudioSource source, int bufferSize = 4096) : base(source)
        {
            buffer = new UnionArray()
            {
                Bytes = new byte[bufferSize]
            };
            int samplePerSec = source.Format.Channels * source.Format.SampleRate * source.Format.BitDepth / 8;
            ringBuffer = new RingBuffer<float>((uint)(samplePerSec * 10));
        }

        public override bool CopySource
        {
            get => false;
            set { }
        }

        public bool IsRecording { get; private set; } = false;

        public void StartRecording(Stream output, bool writeWaveHeader, bool leaveOpen)
        {
            if (writeWaveHeader && !output.CanSeek)
            {
                throw new Exception("Output stream does not support seek opearation");
            }
            
            if (IsRecording)
            {
                return;
            }
            
            IsRecording = true;
            thread = new Thread(() => RunLoop(output, writeWaveHeader, leaveOpen));
            ringBuffer.Clear();
            thread.Start();
        }

        public void StopRecording()
        {
            IsRecording = false;
            lock (lockObj)
            {
            }
        }

        private void RunLoop(Stream stream, bool writeWaveHeader, bool leaveOpen)
        {
            const int headerSize = 44;
            const long riffSizePos = 4;
            const long dataSizePos = 40;
            int rawSize = 0;
            
            lock (lockObj)
            {
                if (writeWaveHeader)
                {
                    using (var writer = new BinaryWriter(stream, Encoding.ASCII, true))
                    {
                        WriteEachChar(writer, "RIFF");
                        writer.Write(new int());
                        WriteEachChar(writer, "WAVE");
                        WriteEachChar(writer, "fmt ");
                        writer.Write(16);
                        writer.Write((short)1);
                        writer.Write((short)Source.Format.Channels);
                        writer.Write(Source.Format.SampleRate);
                        writer.Write(Source.Format.SampleRate * Source.Format.Channels * 16 / 8);
                        writer.Write((short)(Source.Format.Channels * 16 / 8));
                        writer.Write((short)16);
                        WriteEachChar(writer, "data");
                        writer.Write(new int());
                    }
                }

                // Convert 32bit floating-buffer to 16bit buffer
                var bufferSize = buffer.Bytes.Length / 2;
                var floatBuffer = new float[bufferSize];
                while (IsRecording)
                {
                    var read = ringBuffer.Dequeue(floatBuffer, bufferSize);
                    for (int i = 0; i < read; i++)
                    {
                        if (floatBuffer[i] >= 0)
                        {
                            buffer.Shorts[i] = (short) (floatBuffer[i] * short.MaxValue);
                        }
                        else
                        {
                            buffer.Shorts[i] = (short) (floatBuffer[i] * (short.MaxValue + 1));
                        }
                    }
                    
                    if (read > 0)
                    {
                        var size = (int) (read * 2);
                        stream.Write(buffer.Bytes, 0, size);
                        rawSize += size;
                    }
                    else
                    {
                        Thread.Yield();
                    }
                }

                if (writeWaveHeader)
                {
                    using (var writer = new BinaryWriter(stream, Encoding.ASCII, true))
                    {
                        stream.Seek(riffSizePos, SeekOrigin.Begin);
                        writer.Write(rawSize + headerSize - 8);
                        stream.Seek(dataSizePos, SeekOrigin.Begin);
                        writer.Write(rawSize + headerSize - 126);
                    }
                }

                if (!leaveOpen)
                {
                    stream.Dispose();
                }
            }
        }

        private void WriteEachChar(BinaryWriter writer, string chars)
        {
            foreach (var c in chars)
            {
                writer.Write(c);
            }
        }

        protected override void ProcessBuffer(float[] buffer)
        {
            if (IsRecording)
            {
                ringBuffer.Enqueue(buffer);
            }
        }
    }
}
