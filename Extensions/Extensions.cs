using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace Launcher.Extensions
{
    public static class Extensions
    {
        public static IEnumerable<IEnumerable<T>> Transpose<T>(this IEnumerable<IEnumerable<T>> source)
        {
            var enumerators = source.Select(e => e.GetEnumerator()).ToArray();
            try
            {
                while (enumerators.All(e => e.MoveNext()))
                {
                    yield return enumerators.Select(e => e.Current).ToArray();
                }
            }
            finally
            {
                Array.ForEach(enumerators, e => e.Dispose());
            }
        }

        public static async Task<Stream> ReadAsStreamAsync(this HttpContent content, Action<float>? onProgress)
        {
            var totalBytes = (float)content.Headers.ContentLength!;
            var totalBytesRead = 0L;

            var outputStream = new MemoryStream();

            using (var stream = await content.ReadAsStreamAsync())
            {
                var buffer = new byte[65536];
                var bytesRead = 0;

                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await outputStream.WriteAsync(buffer, 0, bytesRead);
                    totalBytesRead += bytesRead;

                    Dispatcher.UIThread.Invoke(() => onProgress?.Invoke(totalBytesRead / totalBytes));
                }
            }

            Dispatcher.UIThread.Invoke(() => onProgress?.Invoke(1));
            outputStream.Position = 0;
            return outputStream;
        }
    }
}
