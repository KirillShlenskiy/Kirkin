using System.Text;

namespace Kirkin.Media.FFmpeg
{
    /// <summary>
    /// ffmpeg video encoder options.
    /// </summary>
    public abstract class VideoEncoder
    {
        /// <summary>
        /// libx264 video encoder.
        /// </summary>
        public static VideoEncoder Libx264 { get; } = new Libx264VideoEncoder();

        /// <summary>
        /// Passthrough video encoder (direct stream copy).
        /// </summary>
        public static VideoEncoder Copy { get; } = new CopyVideoEncoder();

        private VideoEncoder()
        {
        }

        internal abstract string GetCliArgs(FFmpegClient ffmpeg);

        private sealed class Libx264VideoEncoder : VideoEncoder
        {
            internal override string GetCliArgs(FFmpegClient ffmpeg)
            {
                StringBuilder args = new StringBuilder("-c:v libx264 -profile:v high -preset slow");

                if (ffmpeg.VideoBitrate != 0)
                    args.Append($" -b:v {ffmpeg.VideoBitrate}k -maxrate {ffmpeg.VideoBitrate}k -bufsize {ffmpeg.VideoBitrate * 2}k");

                return args.ToString();
            }
        }

        sealed class CopyVideoEncoder : VideoEncoder
        {
            internal override string GetCliArgs(FFmpegClient ffmpeg)
            {
                return "-c:v copy";
            }
        }
    }
}