using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace MediaStreaming.Handlers
{
    public class ScreenHandler : Handler
    {
        public override string Path => "screen";

        public override bool RequireID => true;

        public override bool RequireRoom => true;

        public override bool RequireWebSocket => true;

        public override string Method => HttpMethods.Get;

        protected override void Execute()
        {
            StreamSocket stream = new StreamSocket("stream-screen", Socket);
            ScreenSharingStream screenSharingStream = new ScreenSharingStream(Caller, stream);
            Caller.Sockets.Add(stream);

            ScreenSharingStreams.Add(screenSharingStream);

            Notification.StartScreenStream(Caller.Room, Caller, screenSharingStream);

            ScreenSharing(screenSharingStream.Id);

            ScreenSharingStreams.Remove(screenSharingStream);
        }

    }
}
