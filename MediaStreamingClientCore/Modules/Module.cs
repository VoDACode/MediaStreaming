﻿using MediaStreamingClientCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Net.WebSockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MediaStreamingClientCore.Modules
{
    public abstract class Module : IModule
    {
        private Client client;
        protected bool _status;
        private string _connectWsRootUrl;
        private string _token;

        protected abstract string ModuleName { get; }

        protected ClientWebSocket Socket = new ClientWebSocket();
        protected Client Client => client;
        protected string Token => _token;

        protected string GetConnectWsUrl => $"{ConnectWsRootUrl}/{ModuleName}?id={client.Id}&token={_token}&room={client.Room}";

        public string ConnectWsRootUrl => _connectWsRootUrl;
        public bool Status => _status;

        public bool IgnoreSSL { get; set; }

        public event MediaNotification OnReceiveData;
        public event Action OnStart;
        public event Action OnStop;

        protected Module(string connectWsRootUrl, ref Client client, string token = null)
        {
            _connectWsRootUrl = connectWsRootUrl;
            this.client = client;
            _token = token;
            Socket.Options.RemoteCertificateValidationCallback = (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => IgnoreSSL;
        }

        public virtual void Start()
        {
            try
            {
                Socket.ConnectAsync(new Uri(GetConnectWsUrl), CancellationToken.None).Wait();
            }catch(Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + GetConnectWsUrl, "d", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            _start();
        }

        public virtual void Stop()
        {
            Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client is close connection.", CancellationToken.None).Wait();
            _stop();
        }

        protected void _start()
        {
            OnStart?.Invoke();
            _status = true;
        }
        protected void _stop()
        {
            OnStop?.Invoke();
            _status = false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffetSize">Buffer limit size from Kb</param>
        /// <returns></returns>
        protected Task startReadStream(uint buffetSize = 150)
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    var buffer = new byte[1024 * buffetSize];
                    BytesList bytes = new BytesList();
                    bytes.InitArrays(1024 * buffetSize);
                    WebSocketReceiveResult result = Socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None).Result;
                    while (!result.CloseStatus.HasValue)
                    {
                        bytes.SetBuffer(buffer, result.Count);
                        OnReceiveData?.Invoke(Socket, bytes);
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
