using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MediaStreamingClientCore.Modules
{
    public abstract class Module : IModule
    {
        private Client client;
        private bool _status;
        private string _connectWsRootUrl;
        private string _token;

        protected abstract string ModuleName { get; }

        protected ClientWebSocket Socket = new ClientWebSocket();
        protected Client Client => client;
        protected string Token => _token;

        protected Thread ListeningThread { private set; get; }

        protected string GetConnectWsUrl => $"{ConnectWsRootUrl}/{ModuleName}?id={client.Id}&token={_token}&room={client.Room}";

        public string ConnectWsRootUrl => _connectWsRootUrl;
        public bool Status => _status;

        public event MediaNotification OnReceiveData;
        public event Action OnStart;
        public event Action OnStop;

        protected Module(string connectWsRootUrl, ref Client client, string token = null)
        {
            _connectWsRootUrl = connectWsRootUrl;
            this.client = client;
            _token = token;
        }

        public virtual void Start()
        {
            try
            {
                Socket.ConnectAsync(new Uri(GetConnectWsUrl), CancellationToken.None).Wait();
            }catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "d", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            _start();
        }

        public virtual void Stop()
        {
            Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client is close connection.", CancellationToken.None).Wait();
            ListeningThread.Join();
            _stop();
        }

        protected void _start()
        {
            startReadStream();
            OnStart?.Invoke();
            _status = true;
        }
        protected void _stop()
        {
            OnStop?.Invoke();
            _status = false;
        }
        private Task startReadStream()
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    var buffer = new byte[1024 * 96];
                    WebSocketReceiveResult result = Socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None).Result;
                    while (!result.CloseStatus.HasValue)
                    {
                        OnReceiveData?.Invoke(Socket, buffer, 0, result.Count);
                        result = Socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None).Result;
                    }
                }
                catch(Exception ex)
                { }
                _stop();
            });    
        }
    }
}
