using Microsoft.AspNetCore.Http;

namespace MediaStreamingService.Handlers
{
    public class SetRoomHandler : Handler
    {
        public override string Path => "set/room";

        public override bool RequireID => true;

        public override bool RequireRoom => true;

        public override bool RequireWebSocket => false;

        public override string Method => HttpMethods.Post;

        protected override void Execute()
        {
            Caller.Room = Room;
            HttpContext.Response.StatusCode = StatusCodes.Status202Accepted;
            HttpContext.Response.WriteAsync("Room edited!");
        }
    }
}
