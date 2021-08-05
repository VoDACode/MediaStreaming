using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaStreamingClientCore.Modules
{
    public sealed class NotificationModule : Module
    {
        protected override string ModuleName => "notification";
        public NotificationModule(string ConnectWsHost, ref Client client) 
            : base(ConnectWsHost, ref client)
        {

        }
    }
}