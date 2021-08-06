using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using MediaStreamingClientCore.Modules;

namespace MediaStreamingClientCore
{
    public delegate void MediaNotification(ClientWebSocket socket, byte[] data, int offset, int count);
    public class MediaStreamingClient
    {
        private bool _isHttps;
        private string _host;
        private uint _port;
        private string _servicePath;
        private string _token;
        private bool _isConnect;
        private Client client;
        private bool _ignoreSSL = false;

        private NotificationModule notification;
        private VoiceModule voice;
        private VideoModule video;
        private ScreenSharingModule screenSharing;
        private ApiClient api;


        public NotificationModule Notification => notification;
        public VoiceModule Voice => voice;
        public VideoModule Video => video;
        public ScreenSharingModule ScreenSharing => screenSharing;

        public bool IgnoreSSL { get => _ignoreSSL; 
            set{
                if (_ignoreSSL == value)
                    return;
                api.IgnoreSSL = value;
                _ignoreSSL = value;
            }
        }

        public bool IsConnect { get => _isConnect; }

        public string ConnectHttpRootUrl { get => $"{(_isHttps ? "https" : "http")}://{_host}:{_port}/{_servicePath}"; }
        public string ConnectWsRootUrl { get => $"{(_isHttps ? "wss" : "ws")}://{_host}:{_port}/{_servicePath}"; }

        public event Action OnStart;
        public event Action OnStop;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isHttps">Exemple: your url = 'htpps://localhost:5948/service/streams', then isHttps = true</param>
        /// <param name="host">Exemple: your url = 'htpps://localhost:5948/service/streams', then host = 'localhost'</param>
        /// <param name="port">Exemple: your url = 'htpps://localhost:5948/service/streams', then port = '5948'</param>
        /// <param name="servicePath">Exemple: your url = 'htpps://localhost:5948/service/streams', then servicePath = 'service/streams'</param>
        /// <param name="token">If authorization is configured on the server, then specify the value "token"</param>
        public MediaStreamingClient(bool isHttps, string host, uint port, string servicePath, string token = null)
        {
            _isHttps = isHttps;
            _host = host;
            _port = port;
            _servicePath = servicePath;
            _token = token;
            api = new ApiClient(ConnectHttpRootUrl, _token);
        }

        public void Connect()
        {
            client = api.Logon().Result;

            notification = new NotificationModule(ConnectWsRootUrl, ref client);
            voice = new VoiceModule(ConnectWsRootUrl, ref client);
            video = new VideoModule(ConnectWsRootUrl, ref client);
            screenSharing = new ScreenSharingModule(ConnectWsRootUrl, ref client);

            _isConnect = true;
            OnStart?.Invoke();
        }

        public void Disconnect()
        {
            notification.Stop();
            voice.Stop();
            video.Stop();
            screenSharing.Stop();

            _isConnect = false;
            OnStop?.Invoke();
        }

        public bool SetRoom(string room) 
        {
            var result = api.SetRoom(room, client.Id);
            if (result)
                client.Room = room;
            return result;
        }
        
    }
}
