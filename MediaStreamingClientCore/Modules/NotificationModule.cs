using MediaStreamingClientCore.Models;
using System.Net.WebSockets;
using System.Text;

namespace MediaStreamingClientCore.Modules
{
    public delegate void NotificationAction(NotificationModel data);
    public sealed class NotificationModule : Module
    {
        protected override string ModuleName => "notification";

        public event NotificationAction OnStartMessage;
        public event NotificationAction OnNewconnect;
        public event NotificationAction OnCall;
        public event NotificationAction OnClientLeave;
        public event NotificationAction OnStartScreenStream;
        public event NotificationAction OnStartVideoStream;
        public event NotificationAction OnEndScreenStream;
        public event NotificationAction OnEndVideoStream;
        public event NotificationAction OnOther;

        public NotificationModule(string ConnectWsHost, ref Client client) 
            : base(ConnectWsHost, ref client)
        {
            OnReceiveData += NotificationModule_OnReceiveData;
        }

        public override void Start()
        {
            base.Start();
            startReadStream();
        }

        private void NotificationModule_OnReceiveData(ClientWebSocket socket, BytesList bytes)
        {
            var str = Encoding.UTF8.GetString(bytes.NewBuffer, 0, bytes.NewBuffer.Length);
            var notification = new NotificationModel(str);
            switch (notification.Type)
            {
                case "StartVideoStream":
                    OnStartVideoStream?.Invoke(notification);
                    break;
                case "StartScreenStream":
                    OnStartScreenStream?.Invoke(notification);
                    break;
                case "client_leave":
                    OnClientLeave?.Invoke(notification);
                    break;
                case "new_connect":
                    OnNewconnect?.Invoke(notification);
                    break;
                case "call":
                    OnCall?.Invoke(notification);
                    break;
                case "EndScreenStream":
                    OnEndScreenStream?.Invoke(notification);
                    break;
                case "start":
                    OnStartMessage?.Invoke(notification);
                    break;
                case "EndVideoStream":
                    OnEndVideoStream?.Invoke(notification);
                    break;
                default:
                    OnOther?.Invoke(notification);
                    break;
            }

        }
    }
}