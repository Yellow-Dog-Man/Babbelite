using Babbelite.Shared;
using EchoSharp;
using EchoSharp.Audio;
using EchoSharp.SpeechTranscription;
using EchoSharp.Whisper.net;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Babbelite.Server.Core
{
    public class LiveTranscriptionSession : IDisposable
    {
        public string SessionId { get; private set; }

        BabbeliteServer _server;

        WhisperSpeechTranscriptorFactory _whisperFactory;
        EchoSharpRealtimeTranscriptorFactory _transcriptorFactory;

        AwaitableAudioSource _source;
        IRealtimeSpeechTranscriptor _transcriptor;

        CancellationTokenSource _cancellation;

        public LiveTranscriptionSession(string sessionId, BabbeliteServer server)
        {
            SessionId = sessionId;

            _server = server;

            _source = new AwaitableAudioSource();
            _source.Initialize(new AudioSourceHeader()
            {
                BitsPerSample = sizeof(float),
                Channels = 1,
                SampleRate = 16000
            });

            _whisperFactory = new WhisperSpeechTranscriptorFactory(server.Whisper.ModelPath);
            _transcriptorFactory = new EchoSharpRealtimeTranscriptorFactory(_whisperFactory, server.VadDetectorFactory, echoSharpOptions: new EchoSharpRealtimeOptions()
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

            _source.AddFrame(data);
        }

        public void Dispose()
        {
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
            update.IsCompleted = isCompleted;

            update.Text = segment.Text;
            update.ConfidenceLevel = segment.ConfidenceLevel;
            update.LanguageCode = segment.Language?.Name;

            _server.SendResponse(update);
        }
    }
}
