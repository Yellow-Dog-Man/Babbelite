using Babbelite.Shared;
using EchoSharp.VoiceActivityDetection;
using System;
using System.Collections.Generic;
using System.Text;
using WatsonWebsocket;

namespace Babbelite.Server.Core
{
    public class BabbeliteServer
    {
        WatsonWsServer _server;

        // All the currently connected clients to this server
        Dictionary<ClientMetadata, ClientSession> _sessions = new Dictionary<ClientMetadata, ClientSession>();

        #region TRANSCRIBING

        public WhisperConfig Whisper { get; private set; }

        public IVadDetectorFactory VadDetectorFactory { get; private set; }

        #endregion

        public BabbeliteServer(int port, WhisperConfig whisper)
        {
            _server = new WatsonWsServer("localhost", port);

            _server.ClientConnected += ClientConnected;
            _server.ClientDisconnected += ClientDisconnected;
            _server.MessageReceived += MessageReceived;

            _server.Start();
        }

        void MessageReceived(object? sender, MessageReceivedEventArgs e)
        {
            _sessions[e.Client].HandleMessage(e);
        }

        void ClientDisconnected(object? sender, DisconnectionEventArgs e)
        {
            _sessions[e.Client].Dispose();
            _sessions.Remove(e.Client);
        }

        void ClientConnected(object? sender, ConnectionEventArgs e)
        {
            var session = new ClientSession(this);
            _sessions.Add(e.Client, session);
        }

        internal void SendResponse(Response response)
        {
            throw new NotImplementedException();
        }
    }
}
