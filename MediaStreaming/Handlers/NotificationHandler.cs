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

        public override string Method => "GET";

        protected override void Execute()
        {
            Caller.Sockets.Add(new StreamSocket("notification", Socket));
            Socket.SendAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { text = "OK" })), WebSocketMessageType.Text, true, CancellationToken.None);
            WaitStream();
            Clients.Remove(Caller);
        }
    }
}
