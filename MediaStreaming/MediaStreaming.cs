using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediaStreaming.Handlers;
using MediaStreaming.Handlers.API;
using MediaStreaming.Modules;
using Microsoft.AspNetCore.Http;

namespace MediaStreaming
{
    public delegate void ActinoClientStream(Client client, StreamSocket stream);
    public delegate void ActionClient(Client client);
    public delegate bool ActionCheckAccess(Client client, string room);
    public delegate bool EventAuth(string token);
    public class MediaStreaming
    {
        private static List<ScreenSharingStream> screenSharingViewers = new List<ScreenSharingStream>();
        private static NotificationModule notification;
        private static List<Client> clients = new List<Client>();
        private readonly RequestDelegate _next;
        private string path;
        private MediaStreamingSettings settings;
        private List<Handler> handlers = new List<Handler>();
        public MediaStreaming(RequestDelegate next, string path, MediaStreamingSettings settings = null)
        {
            notification = new NotificationModule(ref clients);
            _next = next;
            if (path.Last() != '/')
                path = $"/{path}";
            this.path = path;
            if(settings == null)
                settings = new MediaStreamingSettings();
            this.settings = settings;

            if (settings.EnableAPI)
            {
                handlers.Add(new ScreenSharingApi());
            }
            if (settings.EnableScreenSharing)
            {
                handlers.Add(new ScreenHandler());
            }
            if (settings.EnableVideo)
            {
                handlers.Add(new VideoHandler());
            }
            if(settings.EnableVideo || settings.EnableScreenSharing)
            {
                handlers.Add(new StreamViewHandler());
            }
            if (settings.EnableVoice)
            {
                handlers.Add(new VoiceHandler());
                handlers.Add(new VoiceListeningHandler());
            }
            
            handlers.Add(new LogonHandler());
            handlers.Add(new SetRoomHandler());
            handlers.Add(new NotificationHandler());
            handlers.Add(new SubscribeHandler());
            handlers.Add(new UnsubscribeHandler());                     
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!isValidRootPath(context.Request.Path))
            {
                await _next.Invoke(context);
                return;
            }

            if (!settings.Auth(context.Request.Query["token"]))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }

            foreach (var handler in handlers)
            {
                if (handler.IsValidPath(path, context.Request.Path))
                {
                    handler.Invoke(context, ref clients, ref notification, ref screenSharingViewers, ref settings);

                    var remoweClients = clients.Where(p => p.Sockets.Where(p => p.Name != "notification")
                                                            .Count(s => s.Socket.CloseStatus.HasValue) >= p.Sockets.Count(p => p.Name != "notification") &&
                                                        (p.LastActive - p.CreateAt) > TimeSpan.FromMinutes(10));
                    foreach (var rc in remoweClients)
                    {
                        var subClients = notification.Subscriptions.Where(p => p.Client.Id == rc.Id);
                        foreach (var subClient in subClients)
                            notification.Subscriptions.Remove(subClient);

                        clients.Remove(rc);
                    }
                    return;
                }
            }

            context.Response.StatusCode = StatusCodes.Status404NotFound;
        }

        private bool isValidRootPath(string path)
        {
            var tmp = path.Split('/').Where(p => !string.IsNullOrWhiteSpace(p)).ToArray();
            string myPath = "";
            for (int i = 0; i < tmp.Length; i++)
            {
                myPath += $"/{tmp[i]}";
                if (myPath == this.path)
                    return true;
            }
            return false;
        }
    }
}
