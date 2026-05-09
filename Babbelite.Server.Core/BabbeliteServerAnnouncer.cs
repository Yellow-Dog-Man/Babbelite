using Babbelite.Shared;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Babbelite.Server.Core
{
    public class BabbeliteServerAnnouncer : IDisposable
    {
        public BabbeliteServer Server { get; private set; }

        List<UdpClient> announcers = new List<UdpClient>();
        List<IPAddress> localAddresses = new List<IPAddress>();
        IPEndPoint announceEndpoint;

        CancellationTokenSource cancellationTokenSource;

        public BabbeliteServerAnnouncer(BabbeliteServer server)
        {
            this.Server = server;

            cancellationTokenSource = new CancellationTokenSource();

            foreach (var ip in NetUtils.GetLocalIPs())
            {
                try
                {
                    var announcer = new UdpClient(new IPEndPoint(ip, 0));
                    announcer.EnableBroadcast = true;

                    localAddresses.Add(ip);

                    announcers.Add(announcer);
                }
                catch (SocketException ex)
                {
                    // do nothing, just don't use that endpoint for announcements
                    // TODO!!! Make some check to avoid catching exception?
                }
            }

            announceEndpoint = new IPEndPoint(IPAddress.Broadcast, BabbeliteServerInfo.ANNOUNCE_PORT);

            // Start the announcer loop
            Task.Run(async () => Announce(cancellationTokenSource.Token));
        }

        async Task Announce(CancellationToken cancellationToken)
        {
            while(!cancellationToken.IsCancellationRequested)
            {
                AnnounceServer();
                await Task.Delay(BabbeliteServerInfo.ANNOUNCE_INTERVAL);
            }
        }

        void AnnounceServer()
        {
            var info = new BabbeliteServerInfo();

            info.UniqueServerID = Server.UniqueID;
            info.Port = Server.Port;
            info.ServerName = Server.ServerName;

            using (var memory = new MemoryStream())
            {
                System.Text.Json.JsonSerializer.Serialize(memory, info);
                Broadcast(memory);
            }
        }

        void Broadcast(MemoryStream stream)
        {
            var data = stream.GetBuffer();

            List<UdpClient> invalidAnnouncers = null;

            foreach (var announcer in announcers)
            {
                try
                {
                    announcer.Send(data, (int)stream.Length, announceEndpoint);
                }
                catch (Exception ex)
                {
                    if (invalidAnnouncers == null)
                        invalidAnnouncers = new List<UdpClient>();

                    invalidAnnouncers.Add(announcer);
                }
            }

            if (invalidAnnouncers != null)
                announcers.RemoveAll(a => invalidAnnouncers.Contains(a));
        }

        public void Dispose()
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
        }
    }
}
