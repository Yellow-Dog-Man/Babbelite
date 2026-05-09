using Babbelite.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Babbelite.Client
{
    // TODO!!! This is pretty much copied from ResoniteLink, since it's useful for this as well.
    // This functionality should be separated into its own generic library and used in both projects
    // to avoid code duplication, but right now I just want to get stuff to work without introducing another repo
    public class BabbeliteServerListener : IDisposable
    {
        public event Action<BabbeliteServerInfo> ServerDiscovered;
        public event Action<BabbeliteServerInfo> ServerUpdated;
        public event Action<BabbeliteServerInfo> ServerGone;

        public void GetDiscoveredSessions(List<BabbeliteServerInfo> sessions)
        {
            lock (_servers)
                sessions.AddRange(_servers.Values);
        }

        Dictionary<string, BabbeliteServerInfo> _servers = new Dictionary<string, BabbeliteServerInfo>();

        UdpClient _listener;

        CancellationTokenSource cancellationTokenSource;

        public BabbeliteServerListener()
        {
            cancellationTokenSource = new CancellationTokenSource();
        }

        public void Start()
        {
            if (_listener != null)
                throw new InvalidOperationException("Listener is already started");

            _listener = new UdpClient();
            _listener.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _listener.Client.Bind(new IPEndPoint(IPAddress.Any, BabbeliteServerInfo.ANNOUNCE_PORT));

            Task.Run(async () => await ReceiveAnnoucements(cancellationTokenSource.Token).ConfigureAwait(false));
            Task.Run(async () => await ProcessCleanups(cancellationTokenSource.Token).ConfigureAwait(false));
        }

        async Task ProcessCleanups(CancellationToken cancellation)
        {
            while (!cancellation.IsCancellationRequested)
            {
                await Task.Delay(BabbeliteServerInfo.ANNOUNCE_INTERVAL);
                ExpireSessions();
            }
        }

        async Task ReceiveAnnoucements(CancellationToken cancellation)
        {
            var endpoint = new IPEndPoint(IPAddress.Any, 0);

            while (!cancellation.IsCancellationRequested)
            {
                try
                {
                    var data = await _listener.ReceiveAsync().ConfigureAwait(false);

                    if (data.Buffer != null)
                        Decode(data.RemoteEndPoint, data.Buffer);
                }
                catch (OperationCanceledException)
                {
                    // ignore, this is fine, just means that the receiving was canceled
                }
                catch (Exception ex)
                {
                    // Swallow the exceptions
                    // Normally this isn't best practice, but this is simple enough mechanism where little can legitimately break
                    // Most cases it will be some networking error or possibly parsing error for the JSON or the data
                    // We want to ignore those and keep receiving data, because it's better than the discovery just stopping to work
                    // due to some unexpected error
                }
            }
        }

        void ExpireSessions()
        {
            lock (_servers)
            {
                List<string> _expiredKeys = null;

                foreach (var session in _servers)
                    if (IsExpired(session.Value))
                    {
                        ServerGone?.Invoke(session.Value);

                        if (_expiredKeys == null)
                            _expiredKeys = new List<string>();

                        _expiredKeys.Add(session.Key);
                    }

                if (_expiredKeys != null)
                    foreach (var key in _expiredKeys)
                        _servers.Remove(key);
            }
        }

        // Consider them expired if they failed to send in 2.5x of the normal annouce interval
        bool IsExpired(BabbeliteServerInfo session) => (DateTime.UtcNow - session.LastUpdateTimestamp).TotalSeconds > BabbeliteServerInfo.ANNOUNCE_INTERVAL.TotalSeconds * 2.5f;

        void Decode(IPEndPoint endpoint, byte[] data)
        {
            using (var stream = new MemoryStream(data))
            {
                var sessionInfo = System.Text.Json.JsonSerializer.Deserialize<BabbeliteServerInfo>(stream);

                // Ignore if the UniqueServerID is not set. This is not a valid server info.
                if (string.IsNullOrEmpty(sessionInfo.UniqueServerID))
                    return;

                lock (_servers)
                {
                    sessionInfo.LastUpdateTimestamp = DateTime.UtcNow;
                    sessionInfo.EndPoint = endpoint;

                    if (sessionInfo.Port < 0)
                    {
                        // This indicates that the session is closed, remove it
                        if (_servers.Remove(sessionInfo.UniqueServerID))
                            ServerGone?.Invoke(sessionInfo);

                        return;
                    }

                    if (_servers.TryGetValue(sessionInfo.UniqueServerID, out var existingInfo))
                    {
                        // Update the session info
                        _servers[sessionInfo.UniqueServerID] = sessionInfo;
                        ServerUpdated?.Invoke(sessionInfo);
                    }
                    else
                    {
                        _servers.Add(sessionInfo.UniqueServerID, sessionInfo);
                        ServerDiscovered?.Invoke(sessionInfo);
                    }
                }
            }
        }

        public void Dispose()
        {
            cancellationTokenSource.Cancel();
            _listener.Dispose();
        }
    }
}
