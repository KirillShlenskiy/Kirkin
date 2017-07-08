namespace Kirkin.Media.FFmpeg
{
    /// <summary>
    /// ffmpeg video encoder options.
    /// </summary>
    public abstract class VideoEncoder
    {
        /// <summary>
        /// libx264 video encoder - slow preset (best quality).
        /// </summary>
        public static VideoEncoder Libx264Slow { get; } = new Libx264VideoEncoder("slow");

        /// <summary>
        /// libx264 video encoder - fast preset (lower quality, faster encoding).
        /// </summary>
        public static VideoEncoder Libx264Fast { get; } = new Libx264VideoEncoder("fast");

        /// <summary>
        /// libx264 video encoder - ultra fast preset (fastest encoding).
        /// </summary>
        public static VideoEncoder Libx264UltraFast { get; } = new Libx264VideoEncoder("ultrafast");

        /// <summary>
        /// Passthrough video encoder (direct stream copy).
        /// </summary>
        public static VideoEncoder Copy { get; } = new CopyVideoEncoder();

        /// <summary>
        /// Video suppressed.
        /// </summary>
        public static VideoEncoder DisableVideo { get; } = new NoVideoEncoder();

        private VideoEncoder()
        {
        }

        internal abstract string GetCliArgs(FFmpegClient ffmpeg);

        private sealed class Libx264VideoEncoder : VideoEncoder
        {
            public string Preset { get; }

            internal Libx264VideoEncoder(string preset)
            {
                Preset = preset;
            }

            internal override string GetCliArgs(FFmpegClient ffmpeg)
            {
                return $"-c:v libx264 -profile:v high -preset {Preset}";
            }
        }

        sealed class CopyVideoEncoder : VideoEncoder
        {
            internal override string GetCliArgs(FFmpegClient ffmpeg)
            {
                return "-c:v copy";
            }
        }

        sealed class NoVideoEncoder : VideoEncoder
        {
            internal override string GetCliArgs(FFmpegClient ffmpeg)
            {
                return "-vn";
            }
        }
    }
}