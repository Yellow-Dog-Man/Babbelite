using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Babbelite.Shared
{
    /// <summary>
    /// Destroys a live transcription session to free up any resources associated with it.
    /// Any unprocessed audio data will be lost.
    /// Use this when you know you won't need to process any more audio for given session (e.g. user disconnects)
    /// It's not necessary to do this when the user stops speaking - just don't send any audio data
    /// </summary>
    public class DestroyLiveTranscribeSession : Message
    {
        /// <summary>
        /// Unique ID of the session to destroy
        /// </summary>
        [JsonPropertyName("sessionId")]
        public string SessionId { get; set; }
    }
}
