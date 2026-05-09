using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;

namespace Babbelite.Shared
{
    public class BabbeliteServerInfo
    {
        public const int ANNOUNCE_PORT = 12567;
        public static readonly TimeSpan ANNOUNCE_INTERVAL = TimeSpan.FromSeconds(10);

        /// <summary>
        /// Name of the session. This is for user and debugging purposes only.
        /// </summary>
        [JsonPropertyName("serverName")]
        public string ServerName { get; set; }

        /// <summary>
        /// Unique ID of the server. This will change between restarts of each server to help distinguish them
        /// </summary>
        [JsonPropertyName("uniqueServerID")]
        public string UniqueServerID { get; set; }

        /// <summary>
        /// Port on which the server is hosting on
        /// </summary>
        [JsonPropertyName("port")]
        public int Port { get; set; }

        /// <summary>
        /// IP endpoint in which the server is is hosted
        /// </summary>
        [JsonIgnore]
        public IPEndPoint EndPoint { get; set; }

        /// <summary>
        /// Timestamp of the last update of this information. This is used to expire sessions
        /// </summary>
        [JsonIgnore]
        public DateTime LastUpdateTimestamp { get; set; }

        [JsonIgnore]
        public Uri URL => new Uri($"ws://{EndPoint.Address}:{Port}");
    }
}
