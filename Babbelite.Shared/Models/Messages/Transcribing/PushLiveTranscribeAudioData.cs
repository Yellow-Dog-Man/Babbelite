using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Babbelite.Shared
{
    /// <summary>
    /// Used to push a chunk of audio data into the live transcribe session.
    /// It's not expected to immediately receive a response - the system can buffer it for as long as it
    /// needs for analysis. Responses are sent asynchronously. 
    /// Generally you'll want to push the audio data into the session as soon as they come in.
    /// </summary>
    public class PushLiveTranscribeAudioData : Message
    {
        /// <summary>
        /// Unique ID of the session into which to push the audio data into
        /// </summary>
        [JsonPropertyName("sessionId")]
        public string SessionId { get; set; }
    }
}
