#nullable enable
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Http
{
    static class JsonHelper
    {
        public static Task WriteAsJsonAsync<TValue>(this HttpResponse response, TValue value)
        {
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");
            return response.WriteAsync(JsonSerializer.Serialize(value));
        }

        public static Task WriteAsJsonAsync<TValue>(this HttpResponse response, TValue value, JsonSerializerOptions? options)
        {
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");
            return response.WriteAsync(JsonSerializer.Serialize(value, options));
        }

        public static Task WriteAsJsonAsync(this HttpResponse response, object? value, Type type)
        {
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");
            return response.WriteAsync(JsonSerializer.Serialize(value, type));
        }

        public static Task WriteAsJsonAsync(this HttpResponse response, object? value, Type type, JsonSerializerOptions? options)
        {
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");
            return response.WriteAsync(JsonSerializer.Serialize(value, type, options));
        }
            
    }
}
