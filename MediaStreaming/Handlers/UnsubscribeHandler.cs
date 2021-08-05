using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MediaStreaming.Handlers
{
    public class UnsubscribeHandler : Handler
    {
        public override string Path => "unsubscribe";

        public override bool RequireID => true;

        public override bool RequireRoom => true;

        public override bool RequireWebSocket => false;

        public override string Method => "DELETE";

        protected override void Execute()
        {
            var subscription = Notification.Subscriptions.FirstOrDefault(p => p.Client.Id == Caller.Id && p.Room == Room);
            if (subscription == null)
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                HttpContext.Response.WriteAsync("No subscription found!");
                return;
            }
            Notification.Subscriptions.Remove(subscription);
            HttpContext.Response.StatusCode = StatusCodes.Status200OK;
            HttpContext.Response.WriteAsync("Subscription deleted");
        }
    }
}
