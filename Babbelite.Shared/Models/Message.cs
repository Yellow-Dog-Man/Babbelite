using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Babbelite.Shared
{
    [JsonDerivedType(typeof(CreateLiveTranscribeSession), "createLiveTranscribeSession")]
    [JsonDerivedType(typeof(DestroyLiveTranscribeSession), "destroyLiveTranscribeSession")]
    [JsonDerivedType(typeof(PushLiveTranscribeAudioData), "pushLiveTranscribeAudioData")]

    [JsonDerivedType(typeof(TranslateText), "translateText")]
    public abstract class Message
    {
        /// <summary>
        /// Optional unique ID of the message. This will be provided in Response to this message.
        /// </summary>
        [JsonPropertyName("messageId")]
        public string MessageID { get; set; }

        public virtual void Validate() { }
    }
}
