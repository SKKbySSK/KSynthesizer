using KSynthesizer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace TestTool.Windows
{
    public static class SynhesizerWaveWriter
    {
        public unsafe static Task WriteAsync(Stream stream, IAudioSource source, TimeSpan duration, int bufferSize = 44100)
        {
            const int headerSize = 44;
            const int bitDepth = 16;
            int dataSize = (int)(source.Format.SampleRate * source.Format.Channels * duration.TotalSeconds * (bitDepth / 8));
            int totalSize = headerSize + dataSize;

            return Task.Run(() =>
            {
                using (var bw = new BinaryWriter(stream, Encoding.ASCII, true))
                {
                    bw.Write(Encoding.ASCII.GetBytes("RIFF"));
                    bw.Write(totalSize - 8);
                    bw.Write(Encoding.ASCII.GetBytes("WAVE"));
                    bw.Write(Encoding.ASCII.GetBytes("fmt "));
                    bw.Write(16);
                    bw.Write((short)1);
                    bw.Write((short)source.Format.Channels);
                    bw.Write(source.Format.SampleRate);
                    bw.Write(source.Format.SampleRate * (bitDepth / 8) * source.Format.Channels);
                    bw.Write((short)(bitDepth / 8 * source.Format.Channels));
                    bw.Write((short)bitDepth);

                    // Data Chunk
                    bw.Write(Encoding.ASCII.GetBytes("data"));
                    bw.Write(dataSize);

                    int left = dataSize / (bitDepth / 8);
                    float[] buffer;
                    short[] writeBuffer = new short[bufferSize];
                    while (left > 0)
                    {
                        int write = Math.Min(left, bufferSize);
                        buffer = source.Next(write);

                        fixed (float* buf = buffer)
                        {
                            for (int i = 0; write > i; i++)
                            {
                                bw.Write((short)(buf[i] * short.MaxValue));
                            }
                        }

                        left -= write;
                    }
                }
            });
        }
    }
}
