#if !NET_40

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Kirkin.IO
{
    /// <summary>
    /// Contains common Stream utility methods.
    /// </summary>
    public static class StreamUtil
    {
        // Taken from .NET source.
        // Largest multiple of 4096 that is still smaller
        // than the large object heap threshold (85K).
        private const int DefaultCopyBufferSize = 81920;

        /// <summary>
        /// Copies data from source to target stream. Unlike the default Stream.CopyToAsync implementation,
        /// performs reads from the source stream and writes to target stream in parallel.
        /// </summary>
        public static async Task ParallelCopyAsync(Stream source, Stream target, int bufferSize = DefaultCopyBufferSize, CancellationToken ct = default)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (bufferSize < 1) throw new ArgumentOutOfRangeException(nameof(bufferSize));

            ct.ThrowIfCancellationRequested();

            byte[] buffer = new byte[bufferSize];
            byte[] standbyBuffer = null; // Allocated only if source is not empty.
            Task writeTask = null;
            int bytesRead;

            while ((bytesRead = await source.ReadAsync(buffer, 0, bufferSize, ct).ConfigureAwait(false)) != 0)
            {
                if (writeTask != null) {
                    await writeTask.ConfigureAwait(false);
                }

                // Any code after WriteAync may execute in parallel with the write.
                // We cannot mutate the buffer until writeTask is awaited.
                writeTask = target.WriteAsync(buffer, 0, bytesRead, ct);

                // Swap buffers.
                byte[] tmp = standbyBuffer;
                standbyBuffer = buffer;
                buffer = tmp ?? new byte[bufferSize];
            }

            if (writeTask != null)
            {
                // Writing final chunk.
                await writeTask.ConfigureAwait(false);
            }
        }
    }
}

#endif