using System;
using System.Collections.Generic;
using System.Text;

namespace Babbelite.Server.Core
{
    public abstract class TranscriptionService
    {
        public abstract LiveTranscriptionSession CreateLiveTranscriptionSession(string sessionId, ClientSession session);
    }
}
