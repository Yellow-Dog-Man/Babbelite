using Babbelite.Shared;
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
        Dictionary<string, LiveTranscriptionSession> _transcribeSessions = new Dictionary<string, LiveTranscriptionSession>();

        BinaryPayloadMessage _waitingBinaryPayload;

        public void HandleMessage(MessageReceivedEventArgs message)
        {
            string sourceMessageId = null;
            Response response;

            try
            {
                switch (message.MessageType)
                {
                    case System.Net.WebSockets.WebSocketMessageType.Binary:
                        if (_waitingBinaryPayload == null)
                            throw new InvalidOperationException($"Unexpected binary message");

                        try
                        {
                            _waitingBinaryPayload.RawBinaryPayload = message.Data.ToArray();
                            response = ProcessMessage(_waitingBinaryPayload);
                        }
                        finally
                        {
                            _waitingBinaryPayload = null;
                        }
                        break;

                    case System.Net.WebSockets.WebSocketMessageType.Text:
                        var deserializedMessage = System.Text.Json.JsonSerializer.Deserialize<Message>(message.Data);

                        if (deserializedMessage == null)
                            throw new InvalidOperationException("Failed to deserialize message");

                        // Store if for later assignment
                        sourceMessageId = deserializedMessage.MessageId;

                        // If it's a binary payload message, we need to defer processing until we get the binary payload
                        if (deserializedMessage is BinaryPayloadMessage binaryPayload)
                        {
                            if (_waitingBinaryPayload != null)
                            {
                                _waitingBinaryPayload = null;
                                throw new InvalidOperationException("Already expecting binary payload message, but got another text message instead");
                            }

                            _waitingBinaryPayload = binaryPayload;
                            return;
                        }
                        else
                            response = ProcessMessage(deserializedMessage);
                        break;

                    case System.Net.WebSockets.WebSocketMessageType.Close:
                        // TODO!!!
                        return;

                    default:
                        throw new NotImplementedException("Unhandled WebSocket Message Type: " + message.MessageType);
                }
            }
            catch(Exception ex)
            {
                // Something went wrong! Send not-ok response
                response = new Response()
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }

            // Send the response
            response.SourceMessageId = sourceMessageId;

            Server.SendResponse(response);
        }

        Response ProcessMessage(Message message)
        {
            switch(message)
            {
                case CreateLiveTranscribeSession createLiveTranscribe:
                    CreateLiveTranscribeSession(createLiveTranscribe);
                    break;

                case DestroyLiveTranscribeSession destroyLiveTranscribeSession:
                    DestroyLiveTranscribeSession(destroyLiveTranscribeSession);
                    break;

                case PushLiveTranscribeAudioData pushLiveTranscribeAudioData:
                    PushLiveTranscribeAudioData(pushLiveTranscribeAudioData);
                    break;
            }

            // If we get here, we just want to send a generic ok response
            // If any of the messages expect a specific response, then they should return it in the switch above
            return new Response()
            {
                IsSuccess = true
            };
        }

        void CreateLiveTranscribeSession(CreateLiveTranscribeSession message)
        {
            if (string.IsNullOrWhiteSpace(message.SessionId))
                throw new ArgumentException("SessionId must be specified");

            if (_transcribeSessions.ContainsKey(message.SessionId))
                throw new InvalidOperationException("SessionId is already in use");

            var session = new LiveTranscriptionSession(message.SessionId, Server);

            _transcribeSessions.Add(message.SessionId, session);
        }

        void DestroyLiveTranscribeSession(DestroyLiveTranscribeSession message)
        {
            if (string.IsNullOrWhiteSpace(message.SessionId))
                throw new ArgumentException("SessionId must be specified");

            if (!_transcribeSessions.TryGetValue(message.SessionId, out var session))
                throw new InvalidOperationException("There is no session with given SessionId");

            // Cleanup the session
            session.Dispose();

            _transcribeSessions.Remove(message.SessionId);
        }

        void PushLiveTranscribeAudioData(PushLiveTranscribeAudioData message)
        {
            if (string.IsNullOrWhiteSpace(message.SessionId))
                throw new ArgumentException("SessionId must be specified");

            if (!_transcribeSessions.TryGetValue(message.SessionId, out var session))
                throw new InvalidOperationException("There is no session with given SessionId");

            session.PushAudioData(message.GetRawAudioData());
        }

        public void Dispose()
        {
            // Dispose all the active sessions if there are any left
            foreach (var session in _transcribeSessions)
                session.Value.Dispose();
        }
    }
}
