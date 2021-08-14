using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MediaStreaming.Handlers.API
{
    public class ScreenSharingApi : ApiHandler
    {
        public override bool RequireID => true;

        public override bool RequireRoom => true;

        public override string Method => HttpMethods.Get;

        protected override string ServiceName => "screen-sharing";

        protected override void Init()
        {
            ModeParameters.Add("list", actionList);
        }

        private void actionList()
        {
            if(Caller.Room != Room)
            {
                sendHttpResponse("Access to this room denied!", StatusCodes.Status401Unauthorized);
                return;
            }
            var result = from s in ScreenSharingStreams
                         where s.Streamer.Room == Room
                         select new
                         {
                             streamId = s.Id,
                             streamerId = s.Streamer.Id,
                             viewersCount = s.Viewers.Count
                         };
            HttpContext.Response.WriteAsJsonAsync(result);
        }
    }
}
