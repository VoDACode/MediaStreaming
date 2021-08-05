using System.Net.Sockets;

namespace MediaStreamingClientCore
{
    class DebugData
    {
        private string Host;
        private int Port;
        private TcpClient tcp;
        private bool IsConnected;
        public DebugData(string host, int port)
        {
            Host = host;
            Port = port;
            tcp = new TcpClient();
        }

        public void Connect()
        {
            tcp.Connect(Host, Port);
            IsConnected = true;
        }

        public void Send(byte[] data)
        {
            try
            {
                if (IsConnected)
                    tcp.GetStream().Write(data, 0, data.Length);
            }
            catch
            {
                IsConnected = false;
            }
        }
    }
}