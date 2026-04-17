using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Babbelite.Client
{
    public class LiveTranscriptionSession : IDisposable
    {
        public BabbeliteConnection Client { get; private set; }

        public LiveTranscriptionSession(BabbeliteConnection client)
        {
            this.Client = client;
        }

        public void PushAudioData(Span<float> samples)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
