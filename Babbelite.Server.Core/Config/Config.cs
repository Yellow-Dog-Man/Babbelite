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
        /// Optional name for the server. Used when presenting list of available servers to the user.
        /// </summary>
        [JsonPropertyName("serverName")]
        public string ServerName { get; set; }

        [JsonPropertyName("hostName")]
        public string HostName { get; set; }

        /// <summary>
        /// On which port to host the server
        /// </summary>
        [JsonPropertyName("port")]
        public int Port { get; set; }

        /// <summary>
        /// Configuration for speech to text transcription system
        /// </summary>
        [JsonPropertyName("transcription")]
        public TranscriptionConfig Transcription { get; set; }

        /// <summary>
        /// Configuration for translation service
        /// </summary>
        [JsonPropertyName("translation")]
        public TranslationConfig Translation { get; set; }
    }
}
