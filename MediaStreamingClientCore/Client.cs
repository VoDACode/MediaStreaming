using System;

namespace MediaStreamingClientCore
{
    public class Client
    {
        public string Id { get; }
        private DateTime _lastActive;
        public string Room { get; set; }
        public DateTime CreateAt { get; }
        public DateTime LastActive { get => _lastActive; }
        public Client(string Id)
        {
            this.Id = Id;
            _lastActive = DateTime.Now;
            CreateAt = DateTime.Now;
        }
        public void UpdateLastActive()
        {
            _lastActive = DateTime.Now;
        }
    }
}
