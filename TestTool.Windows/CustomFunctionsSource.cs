﻿using KSynthesizer.Sources;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestTool.Windows
{
    class CustomFunctionsSource : PeriodicFunctionsSource, ILastBufferRecord
    {
        public CustomFunctionsSource(int sampleRate) : base(sampleRate) { }

        private List<float> lastBuffer = new List<float>();

        public float[] LastBuffer => lastBuffer.ToArray();

        public int BufferLength { get; set; } = 1024;

        public override float[] Next(int size)
        {
            var buffer = base.Next(size);
            lastBuffer.AddRange(buffer);

            if (lastBuffer.Count > BufferLength)
            {
                lastBuffer.RemoveRange(0, lastBuffer.Count - BufferLength);
            }

            return buffer;
        }
    }
}