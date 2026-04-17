using System;
using System.Collections.Generic;
using System.Text;

namespace Babbelite.Client
{
    public class BabbeliteClient
    {
        public LiveTranscriptionSession CreateTranscriptionSession()
        {
            var session = new LiveTranscriptionSession(this);

            throw new NotImplementedException();

            return session;
        }
    }
}
