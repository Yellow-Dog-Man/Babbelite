using Babbelite.Shared;
using EchoSharp.Onnx.SileroVad;
using EchoSharp.VoiceActivityDetection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Babbelite.Server.Core
{
    public class WhisperTranscriptionService : TranscriptionService
    {
        public string ModelPath { get; private set; }
        public IVadDetectorFactory VadDetectorFactory { get; private set; }

        public WhisperTranscriptionService(WhisperConfig config)
        {
            ModelPath = config.WhisperModelPath;

            VadDetectorFactory = new SileroVadDetectorFactory(new SileroVadOptions(config.SileroVadModelPath)
            {
                Threshold = 0.2f, // The threshold for Silero VAD. The default is 0.5f.
                ThresholdGap = 0.1f, // The threshold gap for Silero VAD. The default is 0.15f.
            });
        }

        public override LiveTranscriptionSession CreateLiveTranscriptionSession(string sessionId, ClientSession session)
        {
            return new WhisperLiveTranscriptionSession(sessionId, this, session);
        }
    }
}
