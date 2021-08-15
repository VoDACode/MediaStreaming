using MediaStreaming;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
            app.UseMediaStreaming("ws", new MediaStreamingSettings()
            {
                CheckAccess = (Client client, string room) =>
                {
                    return true;
                },
                NewConnect = (Client client) =>
                {

                },
                ClientChange = (Client client) =>
                {

                },
                Auth = (string token) =>
                {
                    return true;
                }
            });

            app.UseDefaultFiles();
            app.UseStaticFiles();
        }
    }
}
