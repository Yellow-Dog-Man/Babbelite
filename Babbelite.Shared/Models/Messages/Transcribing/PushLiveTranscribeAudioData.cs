using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Serialization;

namespace Babbelite.Shared
{
    [JsonConverter(typeof(JsonStringEnumConverter<AudioDataEncoding>))]
    public enum AudioDataEncoding
    {
        /// <summary>
        /// Uncompressed raw PCM samples. These are simple to work with, but not very network efficient.
        /// Should be OK on LAN
        /// </summary>
        RawPCM
    }

    /// <summary>
    /// Used to push a chunk of audio data into the live transcribe session.
    /// It's not expected to immediately receive a response - the system can buffer it for as long as it
    /// needs for analysis. Responses are sent asynchronously. 
    /// Generally you'll want to push the audio data into the session as soon as they come in.
    /// </summary>
    public class PushLiveTranscribeAudioData : BinaryPayloadMessage
    {
        /// <summary>
        /// Unique ID of the session into which to push the audio data into
        /// </summary>
        [JsonPropertyName("sessionId")]
        public string SessionId { get; set; }

        /// <summary>
        /// Encoding of the audio data
        /// </summary>
        [JsonPropertyName("encoding")]
        public AudioDataEncoding Encoding { get; set; }

        public void SetRawAudioData(Span<float> samples)
        {
            RawBinaryPayload = new byte[samples.Length * sizeof(float)];
            Encoding = AudioDataEncoding.RawPCM;

            samples.CopyTo(MemoryMarshal.Cast<byte, float>(RawBinaryPayload.AsSpan()));
        }

        public Span<float> GetRawAudioData()
        {
            if (Encoding != AudioDataEncoding.RawPCM)
                throw new InvalidOperationException("The audio data is not raw PCM");

            return MemoryMarshal.Cast<byte, float>(RawBinaryPayload.AsSpan());
        }
    }
}
