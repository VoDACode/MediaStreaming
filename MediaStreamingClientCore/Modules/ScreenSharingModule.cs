using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaStreamingClientCore.Modules
{
    public sealed class ScreenSharingModule : Module
    {
        protected override string ModuleName => "screen";
        public ScreenSharingModule(string ConnectWsHost, ref Client client)
            : base(ConnectWsHost, ref client)
        {

        }
    }
}
