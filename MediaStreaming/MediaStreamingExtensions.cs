using Microsoft.AspNetCore.Builder;

namespace MediaStreaming
{
    public static class MediaStreamingExtensions
    {
        public static IApplicationBuilder UseMediaStreaming(this IApplicationBuilder builder, string path, MediaStreamingSettings settings = null)
        {
            return builder.UseMiddleware<MediaStreaming>(path, settings);
        }
    }
}
