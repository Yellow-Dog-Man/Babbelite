using System;
using System.Collections.Generic;
using System.Text;
using WatsonWebsocket;

namespace Babbelite.Server.Core
{
    public class ClientSession : IDisposable
    {
        public BabbeliteServer Server { get; private set; }

        public ClientSession(BabbeliteServer server)
        {
            this.Server = server;
        }

        // All live transcription sessions
        List<LiveTranscriptionSession> _transcribeSessions = new List<LiveTranscriptionSession>();

        public void HandleMessage(MessageReceivedEventArgs message)
        {

        }

        public void Dispose()
        {
            // Dispose all the active sessions if there are any left
            foreach (var session in _transcribeSessions)
                session.Dispose();
        }
    }
}
