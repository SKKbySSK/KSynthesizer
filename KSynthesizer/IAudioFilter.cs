using System;
using System.Collections.Generic;
using System.Text;

namespace KSynthesizer
{
    public interface IAudioFilter : IAudioSource
    {
        bool CopySource { get; set; }

        IAudioSource Source { get; }
    }
}
