using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MediaStreamingClientCore
{
    class ApiClient
    {
        public string Url { get; }
        public string Token { get; }
        public bool IgnoreSSL { get; set; }

        private HttpMessageHandler GetHttpMessageHandler { 
            get{
                var http = new HttpClientHandler();
                if (IgnoreSSL)
                    http.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => IgnoreSSL;
                return http;
            } 
        }

        public ApiClient(string Url, string Token)
        {
            this.Url = Url;
            this.Token = Token;
        }

        public async Task<Client> Logon()
        {
            var result = post("logon");
            var strRes = await result.Content.ReadAsStringAsync();
            if (result.StatusCode == HttpStatusCode.Created)
            {
                return JsonSerializer.Deserialize<Client>(strRes);
            }
            throw new Exception($"Logon exception: {strRes}\nQuery: {Url}/logon");
        }

        public bool SetRoom(string room, string id)
        {
            var result = post($"set/room?id={id}&room={room}");
            return result.StatusCode == HttpStatusCode.Accepted;
        }

        private HttpResponseMessage post(string service, Dictionary<string, string> data = null)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            using (var http = new HttpClient(GetHttpMessageHandler))
            {
                FormUrlEncodedContent content = new FormUrlEncodedContent((data == null) ? new Dictionary<string, string> { } : data);
                
                var a = http.PostAsync($"{Url}/{service}", content);
                response = a.Result;
            }
            return response;
        }

        private HttpResponseMessage get(string service)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            using (var http = new HttpClient(GetHttpMessageHandler))
            {
                response = http.GetAsync($"{Url}/{service}").Result;
            }
            return response;
        }
    }
}
