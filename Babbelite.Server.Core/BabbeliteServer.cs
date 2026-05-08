using Babbelite.Shared;
using EchoSharp.VoiceActivityDetection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks.Dataflow;
using WatsonWebsocket;

namespace Babbelite.Server.Core
{
    public class BabbeliteServer
    {
        WatsonWsServer _server;

        // All the currently connected clients to this server
        Dictionary<ClientMetadata, ClientSession> _sessions = new Dictionary<ClientMetadata, ClientSession>();

        #region CONNECTION

        public int Port { get; private set; }

        #endregion

        #region TRANSCRIBING

        public Config Config { get; private set; }
        public TranscriptionService Transcription { get; private set; }

        #endregion

        public BabbeliteServer(int port, Config config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            this.Port = port;

            _server = new WatsonWsServer("localhost", port);

            _server.ClientConnected += ClientConnected;
            _server.ClientDisconnected += ClientDisconnected;
            _server.MessageReceived += MessageReceived;

            _server.Logger += msg => Console.WriteLine($"WS: {msg}");

            _server.Start();

            this.Config = config;

            
        }

        void Initialize()
        {
            InitializeTranscription(Config.Transcription);
        }

        void InitializeTranscription(TranscriptionConfig config)
        {
            switch(config)
            {
                case WhisperConfig whisper:
                    Transcription = new WhisperTranscriptionService(whisper);
                    break;

                case null:
                    Console.WriteLine($"No transcription service configured");
                    break;
            }
        }

        void MessageReceived(object? sender, MessageReceivedEventArgs e)
        {
            try
            {
                _sessions[e.Client].HandleMessage(e);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"EXCEPTION handling message:\n{ex}");
            }
        }

        void ClientDisconnected(object? sender, DisconnectionEventArgs e)
        {
            Console.WriteLine($"Client disconnected: {e.Client}\n" + Environment.StackTrace);

            _sessions[e.Client].Dispose();
            _sessions.Remove(e.Client);
        }

        void ClientConnected(object? sender, ConnectionEventArgs e)
        {
            Console.WriteLine($"Client connected: {e.Client}");

            var session = new ClientSession(this, e.Client);
            _sessions.Add(e.Client, session);
        }

        internal void SendResponse(ClientSession session, Response response)
        {
            Task.Run(async () =>
            {
                try
                {
                    if (!await SendResponseAsync(session, response).ConfigureAwait(false))
                        throw new Exception($"Response {response} failed to send");
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Failed to send response: {response}\n{ex}");
                }
            });
        }

        internal Task<bool> SendResponseAsync(ClientSession session, Response response)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(response, SerializationHelper.SerializationOptions);

            return _server.SendAsync(session.Client.Guid, json);
        }
    }
}
