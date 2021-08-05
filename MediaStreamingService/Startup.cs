using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net;
using System.Net.Http;

namespace MediaStreamingService
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseWebSockets();
            app.UseMediaStreaming("ws", (context) =>
            {
                return true;

                var httpClientHandler = new HttpClientHandler();
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) =>
                {
                    return true;
                };
                HttpClient client = new HttpClient(httpClientHandler);
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {context.Request.Query["token"]}");
                var responce = client.GetAsync($"https://api.chat.privatevoda.space:5200/auth/isValid").Result;
                var str = responce.Content.ReadAsStringAsync().Result;
                return bool.Parse(str);
            });

            app.UseDefaultFiles();
            app.UseStaticFiles();
        }
    }
}
