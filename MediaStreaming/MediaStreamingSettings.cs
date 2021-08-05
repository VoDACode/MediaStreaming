using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MediaStreaming
{
    public class MediaStreamingSettings
    {
        public EventAuth Auth { get; set; }
        public bool EnableVoice { get; set; } = true;
        public bool EnableAPI { get; set; } = true;
    }
}
