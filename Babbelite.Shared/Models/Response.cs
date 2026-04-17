using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Babbelite.Shared
{
    [JsonDerivedType(typeof(TranscriptionUpdate), "transcriptionUpdate")]
    public abstract class Response
    {

    }
}
