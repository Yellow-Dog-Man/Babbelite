using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Babbelite.Shared
{
    /// <summary>
    /// Creates a new live trascription session. Sessions are used to isolate audio streams from one another.
    /// Typically you'd want to create a new session for each separate voice.
    /// </summary>
    public class CreateLiveTranscribeSession : Message
    {
        /// <summary>
        /// Unique ID of this session. This will be used to match the audio data and transcribed segments to this session
        /// It is your responsibility to ensure this ID is unique and that you keep using the correct SessionId for subsequent calls
        /// </summary>
        [JsonPropertyName("sessionId")]
        public string SessionId { get; set; }
    }
}
