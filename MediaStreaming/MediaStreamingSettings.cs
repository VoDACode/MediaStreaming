using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MediaStreaming
{
    public class MediaStreamingSettings
    {
        public ActinoClientStream CloseStream { get; set; }
        public ActinoClientStream ConnectStream { get; set; }
        public ActionClient ClientChange { get; set; }
        public ActionClient NewConnect { get; set; }
        public ActionCheckAccess CheckAccess { get; set; } = (Client c, string r) => true;
        public EventAuth Auth { get; set; } = (string token) => true;
        public bool EnableVoice { get; set; } = true;
        public bool EnableScreenSharing { get; set; } = true;
        public bool EnableVideo { get; set; } = true;
        public bool EnableAPI { get; set; } = true;

    }
}
