using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Babbelite.Shared
{
    public class TranscribeSessionCreated : Response
    {
        /// <summary>
        /// Unique ID of the created session
        /// </summary>
        [JsonPropertyName("sessionId")]
        public string SessionId { get; set; }

        /// <summary>
        /// Audio sample rate that the transcribe session expects
        /// The audio data needs to be conformed to match this
        /// </summary>
        [JsonPropertyName("sampleRate")]
        public int SampleRate { get; set; }
    }
}
