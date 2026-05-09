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
    }
}
