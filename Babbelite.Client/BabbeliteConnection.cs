using Babbelite.Shared;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

// Note: This is same structure I used in ResoniteLink
// It might be worth abstracting away some of this pattern into its own library to allow to be easily re-used across projects

namespace Babbelite.Client
{
    public class BabbeliteConnection
    {
        public static readonly JsonSerializerOptions SerializationOptions = new JsonSerializerOptions()
        {
            // Necessary for values like Infinity, NaN and so on
            NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals,
            AllowOutOfOrderMetadataProperties = true,
        };

        const int DEFAULT_BUFFER_SIZE = 1024 * 1024 * 2; // 2 MB

        public bool IsConnected => _client?.State == WebSocketState.Open;
        public Exception FailureException { get; private set; }

        ClientWebSocket _client;
        CancellationTokenSource _cancellation;

        ConcurrentDictionary<string, TaskCompletionSource<Response>> _pendingResponses = new ConcurrentDictionary<string, TaskCompletionSource<Response>>();

        public async Task Connect(Uri target, System.Threading.CancellationToken cancellationToken)
        {
            if (_client != null)
                throw new InvalidOperationException("Client has already been initialized.");

            _client = new ClientWebSocket();

            _cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            await _client.ConnectAsync(target, _cancellation.Token);

            _ = Task.Run(async () => ReceiverHandler(_cancellation.Token));
        }

        async Task ReceiverHandler(CancellationToken cancellationToken)
        {
            try
            {
                byte[] buffer = new byte[DEFAULT_BUFFER_SIZE];

                int receivedBytes = 0;

                while (!cancellationToken.IsCancellationRequested)
                {
                    if (receivedBytes == buffer.Length)
                    {
                        // We need bigger buffer!
                        var newBuffer = new byte[buffer.Length * 2];

                        Array.Copy(buffer, newBuffer, buffer.Length);

                        buffer = newBuffer;
                    }

                    var message = await _client.ReceiveAsync(new ArraySegment<byte>(buffer, receivedBytes, buffer.Length - receivedBytes), cancellationToken);

                    receivedBytes += message.Count;

                    if (!message.EndOfMessage)
                        continue;

                    switch (message.MessageType)
                    {
                        case WebSocketMessageType.Text:
                            var response = System.Text.Json.JsonSerializer.Deserialize<Response>(
                                new MemoryStream(buffer, 0, receivedBytes), SerializationOptions);

                            if (_pendingResponses.TryRemove(response.SourceMessageID, out var completion))
                                completion.SetResult(response);
                            else
                                throw new Exception("There is no pending response with this ID");

                            break;

                        case WebSocketMessageType.Binary:
                            throw new NotSupportedException("Binary messages aren't currently supported");

                        case WebSocketMessageType.Close:
                            _cancellation.Cancel();
                            break;
                    }

                    receivedBytes = 0;
                }
            }
            catch (Exception ex)
            {
                FailureException = ex;
            }

            try
            {
                if (_client.State == WebSocketState.Open)
                    await _client.CloseAsync(FailureException == null ?
                        WebSocketCloseStatus.NormalClosure :
                        WebSocketCloseStatus.InternalServerError,
                        FailureException == null ? "Closing" : "Internal Error", _cancellation.Token);
            }
            finally
            {
                _client.Dispose();
            }
        }

        internal async Task<O> SendMessage<I, O>(I message)
            where I : Message
            where O : Response
        {
            // Validate it first before we do anything else
            message.Validate();

            EnsureMessageID(message);

            var responseCompletion = new TaskCompletionSource<Response>();

            if (!_pendingResponses.TryAdd(message.MessageID, responseCompletion))
                throw new InvalidOperationException("Failed to register MessageID. Did you provide duplicate MessageID?");

            var jsonData = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes((Message)message, SerializationOptions);

            await _client.SendAsync(new ArraySegment<byte>(jsonData),
                WebSocketMessageType.Text, true, _cancellation.Token);

            if (message is BinaryPayloadMessage binaryPayload)
            {
                // We must send the binary payload as well following the message
                await _client.SendAsync(new ArraySegment<byte>(binaryPayload.RawBinaryPayload), WebSocketMessageType.Binary, true,
                    _cancellation.Token);
            }

            // Wait for response to arrive and cast it to the target type if compatible
            return await responseCompletion.Task as O;
        }

        void EnsureMessageID(Message message)
        {
            if (message.MessageID == null)
                message.MessageID = Guid.NewGuid().ToString();
        }

        #region API

        public async Task<LiveTranscriptionSession> CreateTranscriptionSession(string sessionId = null)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
                sessionId = Guid.NewGuid().ToString();

            var session = new LiveTranscriptionSession(this, sessionId);

            var createMessage = new CreateLiveTranscribeSession()
            {
                SessionId = sessionId,
            };

            var response = await SendMessage<CreateLiveTranscribeSession, Response>(createMessage).ConfigureAwait(false);

            if (response.IsSuccess)
                return session;
            else
                throw new Exception($"Failed to create transcription session: {response.ErrorMessage}");
        }

        #endregion
    }
}
