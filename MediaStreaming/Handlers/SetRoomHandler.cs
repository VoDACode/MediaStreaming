using Microsoft.AspNetCore.Http;

namespace MediaStreaming.Handlers
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
            if (Settings.CheckAccess(Caller, Room))
            {
                Caller.Room = Room;
                Settings.ClientChange?.Invoke(Caller);
                sendHttpResponse("Room edited!", StatusCodes.Status202Accepted);
            }
            else
                sendHttpResponse("Access denied!", StatusCodes.Status401Unauthorized);
        }
    }
}
