using System;

namespace Kirkin.Media.FFmpeg
{
    /// <summary>
    /// Exception raised by <see cref="FFmpegClient"/>.
    /// </summary>
    [Serializable]
    public sealed class FFmpegException : Exception
    {
        internal FFmpegException(string message)
            : base(message)
        {
        }

        internal FFmpegException(string message, int hResult)
            : base(message)
        {
            HResult = hResult;
        }
    }
}