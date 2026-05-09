using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Babbelite.Shared
{
    public class TranslatedText : Response
    {
        /// <summary>
        /// The resulting translated text
        /// </summary>
        [JsonPropertyName("text")]
        public string Text { get; set; }

        /// <summary>
        /// Language code of in which the text is.
        /// Some backends support 'auto' to automatically detect
        /// </summary>
        [JsonPropertyName("sourceLanguage")]
        public string SourceLanguage { get; set; }

        /// <summary>
        /// Language code of the target language into which the text should be translated to
        /// </summary>
        [JsonPropertyName("targetLanguage")]
        public string TargetLangoage { get; set; }
    }
}
