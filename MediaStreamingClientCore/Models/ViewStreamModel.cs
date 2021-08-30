using MediaStreaming.Client.Core.Modules;
using MediaStreaming.Core;
using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Net.WebSockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaStreaming.Client.Core.Models
{
    public class ViewStreamModel : IModule
    {
        private bool _status;
        public string Id { get; }
        public Client Client { get; }
        public ClientWebSocket ViewSocket { get; }
        private string token;

        public bool Status => _status;

        public string ConnectWsRootUrl { get; }
        public bool IgnoreSSL { get; set; }

        public event MediaNotification OnReceiveData;
        public event Action OnStart;
        public event Action OnStop;

        public ViewStreamModel(string id, Client client, string connectWsRootUrl, string token)
        {
            Id = id;
            Client = client;
            ConnectWsRootUrl = connectWsRootUrl;
            this.token = token;
            ViewSocket = new ClientWebSocket();
            ViewSocket.Options.RemoteCertificateValidationCallback = (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => IgnoreSSL;
        }

        public void Start()
        {
            ViewSocket.ConnectAsync(new Uri($"{ConnectWsRootUrl}/screen/view?id={Client.Id}&token=${token}&room=${Client.Room}&streamId={Id}"), CancellationToken.None).Wait();
            StartReadStream();
            _status = true;
            OnStart?.Invoke();
        }

        private Task StartReadStream()
        {
            return Task.Run(() =>
            {
                var buffer = new byte[1024 * 150];
                WebSocketReceiveResult result = ViewSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None).Result;
                
                try
                {

                    while (!result.CloseStatus.HasValue)
                    {
                        OnReceiveData?.Invoke(ViewSocket, buffer);
                        result = ViewSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None).Result;
                    }
                }
                catch (Exception ex)
                { }
            });
        }
    }
}
