using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Babbelite.Server.Core
{
    [JsonDerivedType(typeof(LibreTranslateConfig), "libreTranslate")]
    public abstract class TranslationConfig
    {

    }
}
