using System;
using System.Collections.Generic;
using System.Text;

namespace Babbelite.Shared
{
    /// <summary>
    /// Creates a new live trascription session. Sessions are used to isolate audio streams from one another.
    /// Typically you'd want to create a new session for each separate voice.
    /// </summary>
    public class CreateLiveTranscribeSession : Message
    {
    }
}
