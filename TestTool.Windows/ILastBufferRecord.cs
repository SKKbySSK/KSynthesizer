using System;
using System.Collections.Generic;
using System.Text;

namespace TestTool.Windows
{
    interface ILastBufferRecord
    {
        float[] LastBuffer { get; }

        int BufferLength { get; set; }
    }
}
