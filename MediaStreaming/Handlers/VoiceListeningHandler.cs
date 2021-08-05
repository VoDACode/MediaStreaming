using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MediaStreaming.Handlers
{
    public class VoiceListeningHandler : Handler
    {
        public override string Path => "voice/listen";

        public override bool RequireID => true;

        public override bool RequireRoom => true;

        public override bool RequireWebSocket => true;

        public override string Method => "GET";

        protected override void Execute()
        {
            StreamSocket stream = new StreamSocket("voice-listen", Socket);
            Caller.Sockets.Add(stream);
            WaitStream();
            Caller.Sockets.Remove(stream);
        }
    }
}
