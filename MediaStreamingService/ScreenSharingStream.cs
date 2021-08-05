using System;
using System.Collections.Generic;

namespace MediaStreamingService
{
    public class ScreenSharingStream
    {
        public string Id { get; }
        public Client Streamer { get; set; }
        public StreamSocket Stream { get; set; }
        public List<Client> Viewers { get; set; }

        public ScreenSharingStream(Client streamer, StreamSocket stream)
        {
            Id = StringHelper.RandomString(64);
            Streamer = streamer;
            Stream = stream;
            Viewers = new List<Client>();
        }

    }
}
