using Microsoft.AspNetCore.Http;
using System.Linq;

namespace MediaStreaming.Handlers
{
    public class InfoHandler : Handler
    {
        public override string Path => "info";

        public override bool RequireID => false;

        public override bool RequireRoom => true;

        public override bool RequireWebSocket => false;

        public override string Method => "GET";

        protected override void Execute()
        {
            var info = new
            {
                count = Clients.Count(p => p.Room == Caller.Room)
            };
            HttpContext.Response.Headers.Add("Content-Type", "application/json");
            HttpContext.Response.StatusCode = StatusCodes.Status200OK;
            HttpContext.Response.WriteAsJsonAsync(info).Wait();
        }
    }
}
