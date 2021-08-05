using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaStreamingClientCore.Modules
{
    public sealed class VideoModule : Module
    {
        protected override string ModuleName => "video";
        public VideoModule(string ConnectWsHost, ref Client client)
            : base(ConnectWsHost, ref client)
        {

        }
    }
}
