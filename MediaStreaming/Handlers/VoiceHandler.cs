using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;

namespace MediaStreaming.Handlers
{
    public class VoiceHandler : Handler
    {
        public override string Path => "voice/start";

        public override bool RequireID => true;

        public override bool RequireRoom => true;

        public override bool RequireWebSocket => true;

        public override string Method => HttpMethods.Get;

        protected override void Execute()
        {
            var stream = new StreamSocket("voice", Socket);
            Caller.Sockets.Add(stream);
            Settings.ConnectStream?.Invoke(Caller, stream);
            if (Clients.Any(p => p.GetStream("voice") != null && p.Room == Caller.Room && p.Id != Caller.Id))
            {
                Notification.ConnectToRoom(Caller.Room, Caller);
            }
            else
            {
                Notification.CallRoom(Caller.Room, Caller);
            }

            SendToOther("voice", "voice-listen", 98304);
            Notification.EndCall(Caller.Room, Caller);
            Settings.CloseStream?.Invoke(Caller, stream);
            Caller.Room = null;
        }
    }
}
