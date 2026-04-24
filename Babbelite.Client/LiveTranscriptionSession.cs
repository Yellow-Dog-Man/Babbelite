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

        public LiveTranscriptionSession(BabbeliteConnection client, string sessionId)
        {
            this.Client = client;
            this.SessionID = sessionId;
        }

        public Task PushAudioData(ReadOnlySpan<float> samples)
        {
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

        public void Dispose()
        {
            if (IsDisposed)
                throw new InvalidOperationException("This session is already disposed");

            IsDisposed = true;

            // Cleanup the session, but don't wait, we don't want to hold up the disposal
            Task.Run(async () =>
            {
                var response = await Client.SendMessage<DestroyLiveTranscribeSession, Response>(new DestroyLiveTranscribeSession()
                {
                    SessionId = SessionID
                }).ConfigureAwait(false);

                // TODO!!! We should probably log this somewhere instead, because right now the exception will just get eaten
                if (!response.IsSuccess)
                    throw new Exception($"Failed to destroy live transcribe session: {response.ErrorMessage}");
            });
        }
    }
}
