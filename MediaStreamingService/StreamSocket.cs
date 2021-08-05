using System.Net.WebSockets;

namespace MediaStreamingService
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
