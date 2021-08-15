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
            var stream = new StreamSocket("video", Socket);
            Caller.Sockets.Add(stream);
            Settings.ConnectStream?.Invoke(Caller, stream);
            Notification.StartStreamVideo(Caller);
            SendToOther("video", "video-listen", 16*1024);
            Settings.CloseStream?.Invoke(Caller, stream);
            Notification.EndStreamVideo(Caller);
        }
    }
}
