using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Babbelite.Shared
{
    [JsonDerivedType(typeof(TranscriptionUpdate), "transcriptionUpdate")]
    public class Response
    {
        /// <summary>
        /// Indicates if message was processed successfully 
        /// </summary>
        [JsonPropertyName("isSuccess")]
        public bool IsSuccess { get; set; }

        /// <summary>
        /// ID of the message to which this response corresponds to (if any)
        /// </summary>
        [JsonPropertyName("sourceMessageId")]
        public string SourceMessageId { get; set; }

        /// <summary>
        /// In case of failure, this contains information on what went wrong.
        /// </summary>
        [JsonPropertyName("errorMessage")]
        public string ErrorMessage { get; set; }
    }
}
