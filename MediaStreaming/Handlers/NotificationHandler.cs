using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MediaStreaming.Handlers
{
    public class NotificationHandler : Handler
    {
        public override string Path => "notification";

        public override bool RequireID => true;

        public override bool RequireRoom => false;

        public override bool RequireWebSocket => true;

        public override string Method => HttpMethods.Get;

        protected override void Execute()
        {
            var stream = new StreamSocket("notification", Socket);
            Caller.Sockets.Add(stream);
            Settings.ConnectStream?.Invoke(Caller, stream);
            Socket.SendAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { data = new { type = "start", data = "OK" } })), WebSocketMessageType.Text, true, CancellationToken.None);
            WaitStream();
            Settings.CloseStream?.Invoke(Caller, stream);
            Clients.Remove(Caller);
        }
    }
}
