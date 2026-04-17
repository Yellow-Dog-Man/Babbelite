using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Babbelite.Shared
{
    /// <summary>
    /// Whenever transcription processes audio data (either fully or partially) it will send the results
    /// using this message. The updates will be received in sequence, with unique ID.
    /// </summary>
    public class TranscriptionUpdate : Response
    {
        /// <summary>
        /// Unique ID of the session for which the transcription data is
        /// </summary>
        [JsonPropertyName("sessionId")]
        public string SessionId { get; set; }
    }
}
