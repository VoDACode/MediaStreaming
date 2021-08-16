using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediaStreaming.Client.Core.Models
{
    public class NotificationModel
    {
        public string Room { get; }
        public DateTime SendDate { get; }
        public string ClientId { get; }
        public string Type { get; }
        public JToken JsonData { get; }
        public string Json { get; }

        public NotificationModel(string room, DateTime date, string clientId, string type, JObject jsonData)
        {
            Room = room;
            SendDate = date;
            ClientId = clientId;
            Type = type;
            JsonData = jsonData;
        }
        public NotificationModel(JObject value)
        {
            Room = value["room"]?.ToString();
            if (value["data"]["date"] != null)
                SendDate = DateTime.Parse(value["data"]["date"].ToString());
            ClientId = value["data"]["client"]?.ToString();
            Type = value["data"]["type"]?.ToString();
            JsonData = value["data"]["data"];
        }
        public NotificationModel(string json)
        {
            Json = json;
            var value = JObject.Parse(json);
            Room = value["room"]?.ToString();
            if(value["data"]["date"] != null)
                SendDate = DateTime.Parse(value["data"]["date"].ToString());
            ClientId = value["data"]["client"]?.ToString();
            Type = value["data"]["type"]?.ToString();
            JsonData = value["data"]["data"];
        }
    }
}
