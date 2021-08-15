#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace MediaStreaming
{
    public class Client
    {
        [JsonIgnore]
        private List<StreamSocket> _sockets;
        private DateTime _lastActive;
        [JsonIgnore]
        public string Token { get; set; }
        public string Id { get; }
        public string Room { get; set; }
        [JsonIgnore]
        public List<StreamSocket> Sockets { 
            get
            {
                UpdateLastActive();
                return _sockets;
            }
            set
            {
                UpdateLastActive();
                _sockets = value;
            }
        }
        public DateTime CreateAt { get; } 
        public DateTime LastActive { get => _lastActive; }
        public Client(string Id)
        {
            this.Id = Id;
            _sockets = new List<StreamSocket>();
            _lastActive = DateTime.Now;
            CreateAt = DateTime.Now;
        }
        public StreamSocket? GetStream(string name)
        {
            UpdateLastActive();
            return Sockets.FirstOrDefault(p => p.Name == name);
        }
        [JsonIgnore]
        public StreamSocket? NotificationStream => GetStream("notification");
        public void UpdateLastActive()
        {
            _lastActive = DateTime.Now;
        }
    }
}
