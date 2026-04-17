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
        AwaitableAudioSource _source;
        IRealtimeSpeechTranscriptor _transcriptor;

        public LiveTranscriptionSession(BabbeliteServer server)
        {
            _source = new AwaitableAudioSource();
            _source.Initialize(new AudioSourceHeader()
            {
                BitsPerSample = sizeof(float),
                Channels = 1,
                SampleRate = 16000
            });

            var whisperFactory = new WhisperSpeechTranscriptorFactory(server.Whisper.ModelPath);
            var transcriptorFactory = new EchoSharpRealtimeTranscriptorFactory(whisperFactory, server.VadDetectorFactory, echoSharpOptions: new EchoSharpRealtimeOptions()
            {
                ConcatenateSegmentsToPrompt = false,
            });

            _transcriptor = transcriptorFactory.Create(new RealtimeSpeechTranscriptorOptions()
            {
                // TODO!!! This should come from session info
                AutodetectLanguageOnce = false, // Flag to detect the language only once or for each segment
                IncludeSpeechRecogizingEvents = true, // Flag to include speech recognizing events (RealtimeSegmentRecognizing)
                RetrieveTokenDetails = true, // Flag to retrieve token details
                LanguageAutoDetect = true, // Flag to auto-detect the language
                Language = new CultureInfo("en-US"), // Language to use for transcription
            });
        }

        public void PushAudioData(ReadOnlyMemory<float> data)
        {
            _source.AddFrame(data);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        async Task RunTranscription()
        {
            await foreach(var transcription in _transcriptor.TranscribeAsync(_source))
            {
                switch(transcription)
                {
                    case RealtimeSessionStarted started:
                        break;

                    case RealtimeSessionStopped:
                        break;

                    case RealtimeSegmentRecognizing recognizing:
                        break;

                    case RealtimeSegmentRecognized recognized:
                        break;
                }
            }
        }
    }
}
