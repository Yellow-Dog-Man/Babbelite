using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Babbelite.Shared
{
    public static class SerializationHelper
    {
        public static readonly JsonSerializerOptions SerializationOptions = new JsonSerializerOptions()
        {
            // Necessary for values like Infinity, NaN and so on
            NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals,
            AllowOutOfOrderMetadataProperties = true,
        };
    }
}
