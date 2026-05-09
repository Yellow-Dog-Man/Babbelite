using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Babbelite.Server.Core
{
    public class LibreTranslateConfig : TranslationConfig
    {
        [JsonPropertyName("hostURL")]
        public string HostURL { get; set; }

        [JsonPropertyName("apiKey")]
        public string ApiKey { get; set; }
    }
}
