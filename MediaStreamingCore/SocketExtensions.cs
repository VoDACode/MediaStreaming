using System;
using System.IO;
using System.IO.Compression;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaStreaming.Core
{
    public static class SocketExtensions
    {
        public static Task SendCompressedDataAsync(this WebSocket socket, ArraySegment<byte> buffer, bool endOfMessage, CancellationToken cancellationToken)
        {
            return socket.SendAsync(compressedData(buffer), WebSocketMessageType.Binary, endOfMessage, cancellationToken);
        }

        public static Task<WebSocketReceiveResult> ReceiveCompressedDataAsync(this WebSocket socket, ArraySegment<byte> buffer, CancellationToken cancellationToken)
        {
            var tmpBuffer = new byte[buffer.Count];
            var result = socket.ReceiveAsync(tmpBuffer, cancellationToken);
            buffer = decompressedData(tmpBuffer, result.Result.Count);
            return result;
        }

        private static byte[] compressedData(ArraySegment<byte> buffer)
        {
            MemoryStream resultStream = new MemoryStream();
            GZipStream zip = new GZipStream(resultStream, CompressionMode.Compress);    
            zip.Write(buffer.Array, 0, buffer.Count);

            var a = Convert.ToBase64String(resultStream.ToArray());
            var tmp = resultStream.ToArray();
            var b = Convert.ToBase64String(decompressedData(tmp, tmp.Length));

            return resultStream.ToArray();       
        }
        private static byte[] decompressedData(ArraySegment<byte> data, int lenght)
        {
            var tmpBuffer = new byte[data.Count];
            var splitData = new byte[lenght];
            Array.Copy(data.Array, splitData, splitData.Length);

            MemoryStream ms = new MemoryStream(splitData);         
            GZipStream zip = new GZipStream(ms, CompressionMode.Decompress);

            var count = zip.Read(tmpBuffer, 0, tmpBuffer.Length);

            var result = new byte[count];
            Array.Copy(tmpBuffer, result, result.Length);
            ms.Close();
            return result;
        }
    }
}
