using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Babbelite.Shared
{
    public class TranscriptionChunk
    {
        /// <summary>
        /// Indicates if this transcription is completed. When false, this represents a partially transcribed text
        /// When true, this will be the last message for the transcription segment
        /// </summary>
        [JsonPropertyName("isCompleted")]
        public bool IsCompleted { get; set; }

        /// <summary>
        /// Current state of transcribed text
        /// </summary>
        [JsonPropertyName("text")]
        public string Text { get; set; }

        /// <summary>
        /// Level of confidence in this transcription. Can be used to indicate if transcription is low confidence and more likely
        /// to be incorrect and to be taken with grain of salt.
        /// </summary>
        [JsonPropertyName("confidenceLevel")]
        public float? ConfidenceLevel { get; set; }

        /// <summary>
        /// Detected language for the transcribed text
        /// </summary>
        [JsonPropertyName("languageCode")]
        public string LanguageCode { get; set; }

        public override string ToString() => $"[Lang: {LanguageCode}, Confidence: {ConfidenceLevel:P0}, Completed: {IsCompleted}] {Text}";
    }
}
