using Babbelite.Shared;
using EchoSharp;
using EchoSharp.Audio;
using EchoSharp.SpeechTranscription;
using EchoSharp.Whisper.net;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace Babbelite.Server.Core
{
    public class LiveTranscriptionSession : IDisposable
    {
        public string SessionId { get; private set; }

        public BabbeliteServer Server => _clientSession.Server;
        readonly ClientSession _clientSession;

        WhisperSpeechTranscriptorFactory _whisperFactory;
        EchoSharpRealtimeTranscriptorFactory _transcriptorFactory;

        AwaitableAudioSource _source;
        IRealtimeSpeechTranscriptor _transcriptor;

        CancellationTokenSource _cancellation;

        public LiveTranscriptionSession(string sessionId, ClientSession clientSession)
        {
            SessionId = sessionId;

            Console.WriteLine($"Creating LiveTranscriptionSession. ID: {SessionId}");

            _clientSession = clientSession;

            _source = new AwaitableAudioSource();
            _source.Initialize(new AudioSourceHeader()
            {
                BitsPerSample = sizeof(float),
                Channels = 1,
                SampleRate = 16000,
            });

            _whisperFactory = new WhisperSpeechTranscriptorFactory(Server.Whisper.ModelPath);
            _transcriptorFactory = new EchoSharpRealtimeTranscriptorFactory(_whisperFactory, Server.VadDetectorFactory, echoSharpOptions: new EchoSharpRealtimeOptions()
            {
                ConcatenateSegmentsToPrompt = false,
            });

            _transcriptor = _transcriptorFactory.Create(new RealtimeSpeechTranscriptorOptions()
            {
                // TODO!!! This should come from session info?
                AutodetectLanguageOnce = false, // Flag to detect the language only once or for each segment
                IncludeSpeechRecogizingEvents = true, // Flag to include speech recognizing events (RealtimeSegmentRecognizing)
                RetrieveTokenDetails = true, // Flag to retrieve token details
                LanguageAutoDetect = true, // Flag to auto-detect the language
            });

            _cancellation = new CancellationTokenSource();

            // Start the analysis
            Task.Run(async () => await RunTranscription(_cancellation.Token).ConfigureAwait(false));
        }

        public void PushAudioData(ReadOnlyMemory<float> data)
        {
            // Ignore if the cancellation has been requested already
            if (_cancellation.IsCancellationRequested)
                return;

            // Frame is essentially just a single "sample"
            // Since we don't do any stereo processing at this point, just add them individually
            // TODO!!! Create custom audio source that is more efficient that doesn't require this kind of slicing?
            // This should be fast enough, but still...
            for(int i = 0; i < data.Length; i++)
                _source.AddFrame(data.Slice(i, 1));

            _source.NotifyNewSamples();
        }

        public void Dispose()
        {
            Console.WriteLine($"Disposing of LiveTranscriptionSession: {SessionId}");

            _cancellation.Cancel();
        }

        async Task RunTranscription(CancellationToken cancellation)
        {
            try
            {
                await foreach (var transcription in _transcriptor.TranscribeAsync(_source, cancellation))
                {
                    if (cancellation.IsCancellationRequested)
                        break;

                    switch (transcription)
                    {
                        case RealtimeSessionStarted started:
                            break;

                        case RealtimeSessionStopped stopped:
                            break;

                        case RealtimeSegmentRecognizing recognizing:
                            SendTranscriptionUpdate(recognizing.Segment, false);
                            break;

                        case RealtimeSegmentRecognized recognized:
                            SendTranscriptionUpdate(recognized.Segment, true);
                            break;
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Exception when running transcription in session {SessionId}\n{ex}");
            }

            Console.WriteLine($"Cleaning up LiveTranscriptionSession. ID: {SessionId}");

            // Cleanup here. We want to do this here rather in Dispose(), so any remainder processing actually finishes
            _source.Dispose();
            _whisperFactory.Dispose();
            _cancellation.Dispose();
        }

        void SendTranscriptionUpdate(TranscriptSegment segment, bool isCompleted)
        {
            // Ignore if we're cleaning up
            if (_cancellation.IsCancellationRequested)
                return;

            var update = new TranscriptionUpdate();

            update.IsSuccess = true;

            update.SessionId = SessionId;

            var chunk = new TranscriptionChunk();

            chunk.IsCompleted = isCompleted;

            chunk.Text = segment.Text;
            chunk.ConfidenceLevel = segment.ConfidenceLevel;
            chunk.LanguageCode = segment.Language?.Name;

            update.TranscriptionChunk = chunk;

            if (chunk.ConfidenceLevel == null || chunk.ConfidenceLevel == 0)
                chunk.ConfidenceLevel = segment.Tokens?.Average(t => t.Confidence ?? 0) ?? 0;

            _clientSession.SendResponse(update);
        }
    }
}
