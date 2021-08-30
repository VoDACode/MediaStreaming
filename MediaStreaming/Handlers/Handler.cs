using MediaStreaming.Modules;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using MediaStreaming.Core;

namespace MediaStreaming.Handlers
{
    public abstract class Handler
    {
        private Client _caller;
        private HttpContext _context;
        private List<Client> _clients;
        private List<ScreenSharingStream> _screenSharingStreams;
        private NotificationModule _notification;
        private WebSocket _socket;
        private MediaStreamingSettings _settings;
        protected string Token { get => HttpContext.Request.Query["token"]; }
        protected MediaStreamingSettings Settings => _settings;
        /// <summary>
        /// Returns the value of the parameter 'room'.
        /// </summary>
        protected string? Room { get => HttpContext.Request.Query["room"]; }
        /// <summary>
        /// Return this client, if 'RequireID' = true, then null.
        /// </summary>
        protected Client? Caller { get => _caller; }
        protected HttpContext HttpContext { get => _context; }

        /// <summary>
        /// List of all clients.
        /// </summary>
        protected List<Client> Clients { get => _clients; }
        protected NotificationModule Notification { get => _notification; }
        /// <summary>
        /// Return WebSocket, if 'RequireWebSocket' = true, then null.
        /// </summary>
        protected WebSocket? Socket { get => _socket; }
        protected List<ScreenSharingStream> ScreenSharingStreams { get => _screenSharingStreams; }

        /// <summary>
        /// Module path
        /// </summary>
        public abstract string Path { get; }
        /// <summary>
        /// Whether to require parameter 'ID'?
        /// </summary>
        public abstract bool RequireID { get; }
        /// <summary>
        /// Whether to require parameter 'Room'?
        /// </summary>
        public abstract bool RequireRoom { get; }
        /// <summary>
        /// Is the WebSocket protocol required?
        /// </summary>
        public abstract bool RequireWebSocket { get; }
        /// <summary>
        /// HTTP Method: GET/POST/DELETE/PUT/OPTIONS/HEAD/PATCH/TRACE/CONNECT
        /// </summary>
        public abstract string Method { get; }
        protected abstract void Execute();

        public void Invoke(HttpContext context, ref List<Client> clients, ref NotificationModule notification, ref List<ScreenSharingStream> screenSharingStreams, ref MediaStreamingSettings settings)
        {
            _settings = settings;
            _notification = notification;
            _clients = clients;
            _screenSharingStreams = screenSharingStreams;
            _context = context;
            if (!CheckMethod(Method))
                return;
            if (RequireWebSocket && !TryGetWebSocket(out _socket))
                return;
            if (RequireRoom && !CheckRoomParameter())
                return;
            if (RequireID)
            {
                string id;
                if (!TryGetClientId(out id))
                    return;
                var client = _clients.FirstOrDefault(p => p.Id == id);
                if (client == null)
                {
                    sendHttpResponse("User is not logged in.", StatusCodes.Status401Unauthorized);
                    return;
                }
                _caller = client;
            }

            Execute();
        }
        public virtual bool IsValidPath(string rootPath, string path)
        {
            var tmp = path.Split('/').Where(p => !string.IsNullOrWhiteSpace(p)).ToArray();
            string myPath = "";
            for (int i = 0; i < tmp.Length; i++)
            {
                myPath += $"/{tmp[i]}";
                if (myPath == $"{rootPath}/{Path}")
                    return true;
            }
            return false;
        }


        private bool TryGetClientId(out string Id)
        {
            if (string.IsNullOrWhiteSpace(HttpContext.Request.Query["id"]))
            {
                sendHttpResponse("Incorrect parameter 'id'!");
                Id = default;
                return false;
            }
            Id = HttpContext.Request.Query["id"];
            return true;
        }
        private bool CheckRoomParameter()
        {
            if (string.IsNullOrWhiteSpace(HttpContext.Request.Query["room"]))
            {
                sendHttpResponse("Incorrect parameter 'room'!");
                return false;
            }
            return true;
        }
        private bool TryGetWebSocket(out WebSocket socket)
        {
            if (!HttpContext.WebSockets.IsWebSocketRequest)
            {
                socket = default;
                sendHttpResponse("Incorrect protocol! Expected 'WebSocket'.");
                return false;
            }
            socket = HttpContext.WebSockets.AcceptWebSocketAsync().Result;
            return true;
        }
        private bool CheckMethod(string method)
        {
            if (HttpContext.Request.Method != method)
            {
                sendHttpResponse($"Expected '{method}' method.", StatusCodes.Status405MethodNotAllowed);
                return false;
            }
            return true;
        }

        protected void sendHttpResponse(string message, int status = StatusCodes.Status400BadRequest)
        {
            HttpContext.Response.StatusCode = status;
            HttpContext.Response.WriteAsync(message);
        }

        protected void SendToOther(string InputSocket, string OutputSocket, int bufferSize = 1024 * 8)
        {
            if (Caller.GetStream(InputSocket) == null)
                throw new Exception($"Not found stream out name '{InputSocket}'.");

            var stream = Caller.GetStream(InputSocket);
            var socket = stream.Socket;

            var buffer = new byte[bufferSize];
            WebSocketReceiveResult result = socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None).Result;
            while (!result.CloseStatus.HasValue)
            {

                Task.Factory.StartNew(() =>
                {
                    var addressees = Clients.Where(p => p.Room == Caller.Room &&
                                    p.Sockets.Any(s => s.Name == stream.Name) &&
                                    p.Id != Caller.Id);
                    foreach (var addressee in addressees)
                    {
                        try
                        {
                            addressee.GetStream(OutputSocket).Socket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), WebSocketMessageType.Binary, result.EndOfMessage, CancellationToken.None);
                        }
                        catch
                        {
                            if (!addressee.GetStream(OutputSocket).Socket.CloseStatus.HasValue)
                                addressee.GetStream(OutputSocket).Socket.CloseAsync(WebSocketCloseStatus.EndpointUnavailable, null, CancellationToken.None);
                            addressee.Sockets.Remove(addressee.GetStream(OutputSocket));
                        }
                    }
                });
                result = socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None).Result;
            }
            Caller.Sockets.Remove(stream);
        }
        protected void WaitStream()
        {
            var buffer = new byte[1024];
            WebSocketReceiveResult result = Socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None).Result;
            while (!result.CloseStatus.HasValue)
            {
                result = Socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None).Result;
            }
        }

        protected void ScreenSharing(string streamId)
        {
            var screenSharingStream = ScreenSharingStreams.FirstOrDefault(p => p.Id == streamId);
            var stream = screenSharingStream.Stream;
            var socket = stream.Socket;
            var streamName = $"stream-view-{screenSharingStream.Id}";
            var buffer = new byte[1024 * 116];
            WebSocketReceiveResult result = socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None).Result;
            while (!result.CloseStatus.HasValue)
            {
                foreach (var addressee in screenSharingStream.Viewers)
                {
                    try
                    {
                        addressee.GetStream(streamName).Socket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), WebSocketMessageType.Binary, result.EndOfMessage, CancellationToken.None);
                    }
                    catch
                    {
                        if (!addressee.GetStream(streamName).Socket.CloseStatus.HasValue)
                            addressee.GetStream(streamName).Socket.CloseAsync(WebSocketCloseStatus.EndpointUnavailable, null, CancellationToken.None);
                        addressee.Sockets.Remove(addressee.GetStream(streamName));
                    }
                }
                result = socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None).Result;
            }
            Caller.Sockets.Remove(stream);
        }

    }
}
