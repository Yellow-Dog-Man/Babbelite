using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Babbelite.Server.Core
{
    public class LibreTranslateConfig : TranslationConfig
    {
        [JsonProperty("hostURL")]
        public string HostURL { get; set; }

        [JsonProperty("apiKey")]
        public string ApiKey { get; set; }
    }
}
