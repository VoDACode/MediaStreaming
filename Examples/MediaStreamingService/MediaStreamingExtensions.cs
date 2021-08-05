using Microsoft.AspNetCore.Builder;

namespace MediaStreamingService
{
    public static class MediaStreamingExtensions
    {
        public static IApplicationBuilder UseMediaStreaming(this IApplicationBuilder builder, string path, EventAuth auth = null)
        {
            return builder.UseMiddleware<MediaStreaming>(path, auth);
        }
    }
}
