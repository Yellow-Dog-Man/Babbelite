using System;
using System.Collections.Generic;
using System.Text;

namespace Babbelite.Server.Core
{
    public class WhisperConfig : TranscriptionConfig
    {
        public string WhisperModelPath { get; set; }
        public string SileroVadModelPath { get; set; }
    }
}
