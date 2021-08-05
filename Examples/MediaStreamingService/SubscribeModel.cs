using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MediaStreamingService
{
    public class SubscribeModel
    {
        public Client Client { get; set; }
        public string Room { get; set; }
    }
}
