using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;

namespace Babbelite.Client
{
    public class BabbeliteConnection
    {
        ClientWebSocket _client;
        CancellationTokenSource _cancellation;

        public LiveTranscriptionSession CreateTranscriptionSession()
        {
            var session = new LiveTranscriptionSession(this);

            throw new NotImplementedException();

            return session;
        }
    }
}
