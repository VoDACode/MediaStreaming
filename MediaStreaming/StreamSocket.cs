using System.Net.WebSockets;
using MediaStreaming.Core;

namespace MediaStreaming
{
    public class StreamSocket
    {
        public WebSocket Socket { get; set; }
        public string Name { get; }
        public StreamSocket(string name, WebSocket socket)
        {
            Name = name;
            Socket = socket;
        }
    }
}
