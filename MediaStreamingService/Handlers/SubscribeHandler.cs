using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MediaStreamingService.Handlers
{
    public class SubscribeHandler : Handler
    {
        public override string Path => "subscribe";

        public override bool RequireID => true;

        public override bool RequireRoom => true;

        public override bool RequireWebSocket => false;

        public override string Method => "POST";

        protected override void Execute()
        {
            if(Notification.Subscriptions.Any(p => p.Client.Id == Caller.Id && p.Room == Room))
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                HttpContext.Response.WriteAsync("You are already subscribed!");
                return;
            }

            Notification.Subscriptions.Add(new SubscribeModel()
            {
                Client = Caller,
                Room = Room
            });
            HttpContext.Response.StatusCode = StatusCodes.Status201Created;
            HttpContext.Response.WriteAsync("Subscription created");
        }
    }
}
