using Microsoft.AspNetCore.Http;
using System.Linq;

namespace MediaStreaming.Handlers
{
    public class StreamViewHandler : Handler
    {
        public override string Path => "screen/view";

        public override bool RequireID => true;

        public override bool RequireRoom => true;

        public override bool RequireWebSocket => true;

        public override string Method => HttpMethods.Get;

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

            StreamSocket streamView = new StreamSocket($"stream-view-{streamId}", Socket);
            Caller.Sockets.Add(streamView);
            Settings.ConnectStream?.Invoke(Caller, streamView);

            ScreenSharingStreams.FirstOrDefault(p => p.Id == streamId).Viewers.Add(Caller);
            WaitStream();
            if (ScreenSharingStreams.Any(p => p.Id == streamId))
            {
                ScreenSharingStreams.FirstOrDefault(p => p.Id == streamId).Viewers.Remove(Caller);
            }
            Caller.Sockets.Remove(streamView);
            Settings.CloseStream?.Invoke(Caller, streamView);
        }
    }
}
