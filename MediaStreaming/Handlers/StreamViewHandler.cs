using Microsoft.AspNetCore.Http;
using System.Linq;

namespace MediaStreaming.Handlers
{
    public class StreamViewHandler : Handler
    {
        public override string Path => "stream-view";

        public override bool RequireID => true;

        public override bool RequireRoom => true;

        public override bool RequireWebSocket => true;

        public override string Method => "GET";

        protected override void Execute()
        {
            string streamId = HttpContext.Request.Query["streamId"];
            if (string.IsNullOrWhiteSpace(streamId))
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                HttpContext.Response.WriteAsync("Incorrect parameter 'streamId'!");
                return;
            }
            if(!ScreenSharingStreams.Any(p => p.Id == streamId))
            {
                HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                HttpContext.Response.WriteAsync("Stream not found!");
                return;
            }
            Caller.Sockets.Add(new StreamSocket($"stream-view-{streamId}", Socket));
            ScreenSharingStreams.FirstOrDefault(p => p.Id == streamId).Viewers.Add(Caller);
            WaitStream();
        }
    }
}
