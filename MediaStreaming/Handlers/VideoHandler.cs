using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MediaStreaming.Handlers
{
    public class VideoHandler : Handler
    {
        public override string Path => "video";

        public override bool RequireID => true;

        public override bool RequireRoom => false;

        public override bool RequireWebSocket => true;

        public override string Method => HttpMethods.Get;

        protected override void Execute()
        {
            Caller.Sockets.Add(new StreamSocket("video", Socket));
            Notification.StartStreamVideo(Caller);
            SendToOther("video", 16);
        }
    }
}
