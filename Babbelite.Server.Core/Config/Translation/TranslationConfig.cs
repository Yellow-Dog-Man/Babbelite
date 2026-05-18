using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Babbelite.Server.Core
{
    [JsonDerivedType(typeof(LibreTranslateConfig), "libreTranslate")]
    [JsonDerivedType(typeof(DeepLConfig), "deepL")]
    public abstract class TranslationConfig
    {
        [JsonPropertyName("priority")]
        public int Priority { get; set; }
        
        /// <summary>
        /// List of language codes that this translation engine should be preferred for
        /// When there are multiple translation backends which have an overlap in the supported language
        /// This allows specifying which translation services should be used for which languages.
        /// It's recommended to only specify this specificaly for services that are particularly
        /// good at certain language, as that service will be preferred when the language is
        /// either source or target.
        /// </summary>
        [JsonPropertyName("preferredLanguages")]
        public HashSet<string> PreferredLanguages { get; set; }
    }
}
