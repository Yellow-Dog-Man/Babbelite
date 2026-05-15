using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Babbelite.Server.Core
{
    public class WhisperConfig : TranscriptionConfig
    {
        [JsonPropertyName("whisperModelPath")]
        public string WhisperModelPath { get; set; }

        [JsonPropertyName("sileroVadModelPath")]
        public string SileroVadModelPath { get; set; }

        [JsonPropertyName("sileroVadThreshold")]
        public float SileroVadThreshold { get; set; } = 0.1f;

        [JsonPropertyName(("sileroVadThresholdGap"))]
        public float SileroVadThresholdGap { get; set; } = 0.05f;
    }
}
