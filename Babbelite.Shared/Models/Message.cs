using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Babbelite.Shared
{
    [JsonDerivedType(typeof(CreateLiveTranscribeSession), "createLiveTranscribeSession")]
    [JsonDerivedType(typeof(DestroyLiveTranscribeSession), "destroyLiveTranscribeSession")]
    [JsonDerivedType(typeof(PushLiveTranscribeAudioData), "pushLiveTranscribeAudioData")]
    public abstract class Message
    {

    }
}
