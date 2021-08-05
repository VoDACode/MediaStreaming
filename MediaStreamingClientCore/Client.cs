using System;

namespace MediaStreamingClientCore
{
    public class Client
    {
        public string id { get; }
        private DateTime _lastActive;
        public string room { get; set; }
        public DateTime CreateAt { get; }
        public DateTime LastActive { get => _lastActive; }
        public Client(string Id)
        {
            this.id = Id;
            _lastActive = DateTime.Now;
            CreateAt = DateTime.Now;
        }
        public void UpdateLastActive()
        {
            _lastActive = DateTime.Now;
        }
    }
}
