using Microsoft.AspNetCore.Http;

namespace MediaStreaming.Handlers
{
    public class ScreenHandler : Handler
    {
        public override string Path => "screen/stream";

        public override bool RequireID => true;

        public override bool RequireRoom => true;

        public override bool RequireWebSocket => true;

        public override string Method => HttpMethods.Get;

        protected override void Execute()
        {
            StreamSocket stream = new StreamSocket("screen-stream", Socket);
            ScreenSharingStream screenSharingStream = new ScreenSharingStream(Caller, stream);
            Caller.Sockets.Add(stream);
            Settings.ConnectStream?.Invoke(Caller, stream);
            ScreenSharingStreams.Add(screenSharingStream);

            Notification.StartScreenStream(Caller.Room, Caller, screenSharingStream);

            ScreenSharing(screenSharingStream.Id);

            Notification.EndScreenStream(Caller.Room, Caller, screenSharingStream);

            Settings.CloseStream?.Invoke(Caller, stream);
            ScreenSharingStreams.Remove(screenSharingStream);
        }

    }
}
