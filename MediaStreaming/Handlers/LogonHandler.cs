using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MediaStreaming.Handlers
{
    public class LogonHandler : Handler
    {
        public override string Path => "logon";

        public override bool RequireID => false;

        public override bool RequireRoom => false;

        public override bool RequireWebSocket => false;

        public override string Method => HttpMethods.Post;

        protected override void Execute()
        {
            var client = new Client(StringHelper.RandomString(64));
            client.Token = Token;
            Clients.Add(client);
            Settings.NewConnect?.Invoke(client);
            HttpContext.Response.StatusCode = StatusCodes.Status201Created;
            HttpContext.Response.WriteAsJsonAsync(client);
        }
    }
}
