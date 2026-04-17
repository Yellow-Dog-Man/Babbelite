using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;

namespace Babbelite.Client
{
    public class BabbeliteClient
    {
        // Connections to Babbelite servers
        List<BabbeliteConnection> _connections = new List<BabbeliteConnection>();

        public LiveTranscriptionSession CreateTranscriptionSession()
        {
            throw new NotImplementedException();    
        }
    }
}
