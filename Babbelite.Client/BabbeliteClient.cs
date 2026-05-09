using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Babbelite.Client
{
    public class BabbeliteClient
    {
        BabbeliteServerListener _listener;

        // Connections to Babbelite servers
        List<BabbeliteConnection> _connections = new List<BabbeliteConnection>();

        public BabbeliteClient(bool autoConnect)
        {
            _listener = new BabbeliteServerListener();

            if(autoConnect)
                _listener.ServerDiscovered += ServerDiscovered;

            _listener.Start();
        }

        void ServerDiscovered(Shared.BabbeliteServerInfo serverInfo)
        {
            // Connect to the server automatically
            Task.Run(async () =>
            {
                // Connect immediatelly when it's discovered
                await ConnectTo(serverInfo.URL, CancellationToken.None);
            });
        }

        public async Task ConnectTo(Uri uri, CancellationToken token)
        {
            try
            {
                var connection = new BabbeliteConnection();

                await connection.Connect(uri, token);

                lock (_connections)
                    _connections.Add(connection);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Exception connecting to {uri}:\n{ex}");
            }
        }

        public async Task<LiveTranscriptionSession> CreateTranscriptionSession(string customId = null)
        {
            var connection = FindBestConnection();

            if (connection == null)
                throw new InvalidOperationException($"Could not find a free Babbelite server connection");

            return await connection.CreateTranscriptionSession(customId);
        }

        BabbeliteConnection FindBestConnection()
        {
            if (_connections.Count == 0)
                return null;

            BabbeliteConnection best = null;

            foreach(var connection in _connections)
            {
                // TODO!!! Improve the logic to take capacity into account
                // Currently we don't track that, so just pick one that has the least amount
                if (best == null || best.TranscriptionSessionCount > connection.TranscriptionSessionCount)
                    best = connection;
            }

            return best;
        }
    }
}
