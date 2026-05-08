using System;
using System.Collections.Generic;
using System.Text;

namespace Babbelite.Server.Core
{
    public abstract class LiveTranscriptionSession : IDisposable
    {
        public abstract string SessionId { get; }
        public abstract int SampleRate { get; }

        public abstract void PushAudioData(ReadOnlyMemory<float> data);
        public abstract void Dispose();
    }
}
