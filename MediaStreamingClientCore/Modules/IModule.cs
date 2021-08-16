using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace MediaStreaming.Client.Core.Modules
{
    interface IModule
    {
        public event MediaNotification OnReceiveData;
        public event Action OnStart;
        public event Action OnStop;
        public bool Status { get; }
        public string ConnectWsRootUrl { get; }
        public bool IgnoreSSL { get; set; }
    }
}
