using Babbelite.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Babbelite.Client
{
    public class LiveTranscriptionSession : IDisposable
    {
        public BabbeliteConnection Client { get; private set; }
        public string SessionID { get; private set; }

        public bool IsDisposed { get; private set; }

        // This event will be triggered every time there's a transcription update
        public event Action<TranscriptionChunk> TranscriptionUpdated;

        public LiveTranscriptionSession(BabbeliteConnection client, string sessionId)
        {
            this.Client = client;
            this.SessionID = sessionId;
        }

        public Task PushAudioData(ReadOnlySpan<float> samples)
        {
            CheckDisposed();

            var audioData = new PushLiveTranscribeAudioData();

            audioData.SessionId = SessionID;
            audioData.SetRawAudioData(samples);

            // We need to wrap it like this, because ReadOnlySpan can't be used in actual async context
            // However since we already assigned the data above, we can do the rest fine
            return Task.Run(async () =>
            {
                var response = await Client.SendMessage<PushLiveTranscribeAudioData, Response>(audioData).ConfigureAwait(false);

                if (response.IsSuccess)
                    return;

                throw new Exception($"Failed to PushAudioData: {response.ErrorMessage}");
            });
        }

        internal void SendTranscriptionUpdated(TranscriptionChunk chunk)
        {
            // Just trigger the event
            TranscriptionUpdated?.Invoke(chunk);
        }

        public void Dispose()
        {
            CheckDisposed();

            IsDisposed = true;

            Client.RemoveTranscriptionSession(this);
        }

        void CheckDisposed()
        {
            if (IsDisposed)
                throw new InvalidOperationException("This session is has been disposed");
        }
    }
}
