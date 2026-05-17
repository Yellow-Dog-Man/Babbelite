using Babbelite.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks.Dataflow;
using WatsonWebsocket;

namespace Babbelite.Server.Core
{
    public class ClientSession : IDisposable
    {
        public BabbeliteServer Server { get; private set; }
        public ClientMetadata Client { get; private set; }

        public ClientSession(BabbeliteServer server, ClientMetadata client)
        {
            this.Server = server;
            this.Client = client;

            _messageHandler = new ActionBlock<MessageReceivedEventArgs>(HandleMessage,
                new ExecutionDataflowBlockOptions()
                {
                    MaxDegreeOfParallelism = 1,
                    EnsureOrdered = true
                });
        }

        // All live transcription sessions
        Dictionary<string, LiveTranscriptionSession> _transcribeSessions = new Dictionary<string, LiveTranscriptionSession>();

        BinaryPayloadMessage _waitingBinaryPayload;

        ActionBlock<MessageReceivedEventArgs> _messageHandler;

        public void EnqueueMessageForProcessing(MessageReceivedEventArgs message)
        {
            _messageHandler.Post(message);
        }

        public async Task HandleMessage(MessageReceivedEventArgs message)
        {
            try
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
                                sourceMessageId = _waitingBinaryPayload.MessageID;

                                _waitingBinaryPayload.RawBinaryPayload = message.Data.ToArray();
                                response = await ProcessMessage(_waitingBinaryPayload);
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
                            sourceMessageId = deserializedMessage.MessageID;

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
                                response = await ProcessMessage(deserializedMessage);
                            break;

                        case System.Net.WebSockets.WebSocketMessageType.Close:
                            Console.WriteLine($"Client closing session: {Client}");
                            // TODO!!!
                            return;

                        default:
                            throw new NotImplementedException("Unhandled WebSocket Message Type: " + message.MessageType);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception handling websocket message for client {Client}:\n{ex}");

                    // Something went wrong! Send not-ok response
                    response = new Response()
                    {
                        IsSuccess = false,
                        ErrorMessage = ex.Message
                    };
                }

                // Send the response
                response.SourceMessageID = sourceMessageId;

                SendResponse(response);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Exception processing message:\n{ex}");
            }
        }

        async ValueTask<Response> ProcessMessage(Message message)
        {
            switch(message)
            {
                case CreateLiveTranscribeSession createLiveTranscribe:
                    return CreateLiveTranscribeSession(createLiveTranscribe);

                case DestroyLiveTranscribeSession destroyLiveTranscribeSession:
                    DestroyLiveTranscribeSession(destroyLiveTranscribeSession);
                    break;

                case PushLiveTranscribeAudioData pushLiveTranscribeAudioData:
                    PushLiveTranscribeAudioData(pushLiveTranscribeAudioData);
                    break;

                case TranslateText translateText:
                    // Find the best translation service for this request
                    var service = await Server.FindBestTranslationService(translateText.SourceLanguage, translateText.TargetLanguage);

                    if (service == null)
                    {
                        // Nothing was found, so we need to return an error
                        return new TranslatedText()
                        {
                            IsSuccess = false,
                            ErrorMessage = "No suitable translation service found"
                        };
                    }

                    return await service.Translate(translateText).ConfigureAwait(false);
            }

            // If we get here, we just want to send a generic ok response
            // If any of the messages expect a specific response, then they should return it in the switch above
            return new Response()
            {
                IsSuccess = true
            };
        }

        TranscribeSessionCreated CreateLiveTranscribeSession(CreateLiveTranscribeSession message)
        {
            if (string.IsNullOrWhiteSpace(message.SessionId))
                throw new ArgumentException("SessionId must be specified");

            if (_transcribeSessions.ContainsKey(message.SessionId))
                throw new InvalidOperationException("SessionId is already in use");

            var session = Server.Transcription.CreateLiveTranscriptionSession(message.SessionId, this);

            _transcribeSessions.Add(message.SessionId, session);

            return new TranscribeSessionCreated()
            {
                SessionId = message.SessionId,
                SampleRate = session.SampleRate,
                IsSuccess = true
            };
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

        internal void SendResponse(Response response)
        {
            Server.SendResponse(this, response);
        }

        public void Dispose()
        {
            Console.WriteLine($"Disposing of client session: {Client}");

            // Dispose all the active sessions if there are any left
            foreach (var session in _transcribeSessions)
                session.Value.Dispose();
        }
    }
}
