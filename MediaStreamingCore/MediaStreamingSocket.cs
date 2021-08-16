using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Compression;
using System.IO;

namespace MediaStreaming.Core
{
    public class MediaStreamingSocket : WebSocket
    {
        private ClientWebSocket client;

        public override WebSocketCloseStatus? CloseStatus => client.CloseStatus;

        public override string CloseStatusDescription => client.CloseStatusDescription;

        public override WebSocketState State => client.State;

        public override string SubProtocol => client.SubProtocol;

        public override void Abort() => client.Abort();
        public ClientWebSocketOptions Options => client.Options;

        public MediaStreamingSocket()
        {
            client = new System.Net.WebSockets.ClientWebSocket();
        }

        public Task ConnectAsync(Uri uri, CancellationToken cancellationToken) => client.ConnectAsync(uri, cancellationToken);

        public override Task CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
        {
            return client.CloseAsync(closeStatus, statusDescription, cancellationToken);
        }

        public override Task CloseOutputAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
        {
            return client.CloseOutputAsync(closeStatus, statusDescription, cancellationToken);
        }

        public override void Dispose() => client.Dispose();

        public override async Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
        {
            var tmpBuffer = new byte[buffer.Count];
            var result = await client.ReceiveAsync(tmpBuffer, cancellationToken);
            
            using (MemoryStream ms = new MemoryStream(tmpBuffer))
            {
                using (GZipStream zip = new GZipStream(ms, CompressionLevel.Optimal, true))
                {
                    zip.Read(buffer);
                }
            }
            return result;
        }

        public override Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
        {
            using (MemoryStream ms = new MemoryStream(new byte[buffer.Count]))
            {
                using (GZipStream zip = new GZipStream(ms, CompressionLevel.Optimal, true))
                {
                    zip.Write(buffer);
                }
                return client.SendAsync(ms.ToArray(), messageType, endOfMessage, cancellationToken);
            }
        }
    }
}
