using System;
using System.Collections.Generic;
using System.Text;

namespace KSynthesizer
{
    interface IAudioFilter : IAudioSource
    {
        bool CopySource { get; set; }

        IAudioSource Source { get; }
    }
}
