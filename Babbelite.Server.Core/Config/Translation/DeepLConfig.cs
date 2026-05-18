using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Babbelite.Server.Core
{
    public class DeepLConfig : TranslationConfig
    {
        /// <summary>
        /// Authentication key for the DeepL service
        /// </summary>
        [JsonPropertyName("authKey")]
        public string AuthKey { get; set; }
    }
}