using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaStreaming.Modules
{
    sealed public class NotificationModule
    {
        private List<Client> Clients;
        public NotificationModule(ref List<Client> clients)
        {
            Clients = clients;
        }
             
        public List<SubscribeModel> Subscriptions { get; set; } = new List<SubscribeModel>();
        
        public void SendToRoomJson(object data, string room)
        {
            object contect = new
            {
                data = data,
                room = room
            };
            var recipients = Subscriptions.Where(p => p.Room == room);
            foreach(var recipient in recipients)
            {
                recipient.Client.NotificationStream.Socket.SendAsync(buffer: Encoding.UTF8.GetBytes(JsonSerializer.Serialize(contect)),
                                                                    messageType: WebSocketMessageType.Text,
                                                                    endOfMessage: true,
                                                                    cancellationToken: CancellationToken.None);
            }
        }
        
        public Task CallRoom(string room, Client caller)
        {
            return Task.Factory.StartNew(() =>
            {
                DateTime startCallDate = DateTime.Now;
                while (true)
                {
                    var recipients = Subscriptions.Where(p => p.Room == room &&
                                                    p.Client.Room != room &&
                                                    p.Client.Id != caller.Id);
                    if (recipients.Count() == 0 || (DateTime.Now - startCallDate) > TimeSpan.FromMinutes(1))
                        return;
                    foreach (var recipient in recipients)
                    {
                        var contect = new
                        {
                            room = room,
                            data = new
                            {
                                type = "call",
                                caller = caller.Id,
                                room = room,
                                date = startCallDate
                            }
                        };
                        recipient.Client.NotificationStream.Socket.SendAsync(buffer: Encoding.UTF8.GetBytes(JsonSerializer.Serialize(contect)),
                                                                           messageType: WebSocketMessageType.Text,
                                                                           endOfMessage: true,
                                                                           cancellationToken: CancellationToken.None);
                    }
                    Thread.Sleep(2000);
                }
            });         
        }

        public void ConnectToRoom(string room, Client client)
        {
            var recipients = Clients.Where(p => p.Room == room &&
                                               p.Id != client.Id);
            var connectDate = DateTime.Now;
            foreach (var recipient in recipients)
            {
                var contect = new
                {
                    room = room,
                    data = new
                    {
                        type = "new_connect",
                        client = client.Id,
                        date = connectDate
                    }
                };
                recipient.NotificationStream.Socket.SendAsync(buffer: Encoding.UTF8.GetBytes(JsonSerializer.Serialize(contect)),
                                                                   messageType: WebSocketMessageType.Text,
                                                                   endOfMessage: true,
                                                                   cancellationToken: CancellationToken.None);
            }
        }

        public void EndCall(string room, Client client)
        {
            var recipients = Clients.Where(p => p.Room == room &&
                                                p.Id != client.Id);
            var connectDate = DateTime.Now;
            foreach (var recipient in recipients)
            {
                var contect = new
                {
                    room = room,
                    data = new
                    {
                        type = "client_leave",
                        client = client.Id,
                        date = connectDate
                    }
                };
                recipient.NotificationStream.Socket.SendAsync(buffer: Encoding.UTF8.GetBytes(JsonSerializer.Serialize(contect)),
                                                                   messageType: WebSocketMessageType.Text,
                                                                   endOfMessage: true,
                                                                   cancellationToken: CancellationToken.None);
            }
        }

        public void StartScreenStream(string room, Client client, ScreenSharingStream screenSharing)
        {
            var recipients = Clients.Where(p => p.Id != client.Id &&
                                                p.Room == room);
            var connectDate = DateTime.Now;
            foreach (var recipient in recipients)
            {
                var contect = new
                {
                    room = room,
                    data = new
                    {
                        type = "StartScreenStream",
                        client = client.Id,
                        date = connectDate,
                        stream = new
                        {
                            id = screenSharing.Id
                        }
                    }
                };
                recipient.NotificationStream.Socket.SendAsync(buffer: Encoding.UTF8.GetBytes(JsonSerializer.Serialize(contect)),
                                                                   messageType: WebSocketMessageType.Text,
                                                                   endOfMessage: true,
                                                                   cancellationToken: CancellationToken.None);
            }
        }

        public void StartStreamVideo(Client caller)
        {
            var recipients = Clients.Where(p => p.Id != caller.Id &&
                                                p.Room == caller.Room);
            var connectDate = DateTime.Now;
            foreach (var recipient in recipients)
            {
                var contect = new
                {
                    room = caller.Room,
                    data = new
                    {
                        type = "videoStream",
                        client = caller.Id,
                        date = connectDate
                    }
                };
                recipient.NotificationStream.Socket.SendAsync(buffer: Encoding.UTF8.GetBytes(JsonSerializer.Serialize(contect)),
                                                                   messageType: WebSocketMessageType.Text,
                                                                   endOfMessage: true,
                                                                   cancellationToken: CancellationToken.None);
            }
        }
    }
}
