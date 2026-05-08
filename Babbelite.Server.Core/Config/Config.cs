using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Babbelite.Server.Core
{
    /// <summary>
    /// Configuration for the Babbelite server.
    /// This determines which systems & backends are initialized
    /// </summary>
    public class Config
    {
        /// <summary>
        /// Configuration for speech to text transcription system
        /// </summary>
        [JsonPropertyName("transcription")]
        public TranscriptionConfig Transcription { get; set; }
    }
}
