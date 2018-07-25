#if !NETSTANDARD2_0

using System;

namespace Kirkin.Media.FFmpeg
{
    /// <summary>
    /// ffmpeg video encoder options.
    /// </summary>
    public enum VideoEncoder
    {
        /// <summary>
        /// Auto detection (based on output file extension).
        /// </summary>
        Auto,

        /// <summary>
        /// libx264 video encoder - slow preset (best quality).
        /// </summary>
        Libx264Slow,

        /// <summary>
        /// libx264 video encoder - fast preset (lower quality, faster encoding).
        /// </summary>
        Libx264Fast,

        /// <summary>
        /// libx264 video encoder - ultra fast preset (fastest encoding).
        /// </summary>
        Libx264UltraFast,

        /// <summary>
        /// Passthrough video encoder (direct stream copy).
        /// </summary>
        Copy,

        /// <summary>
        /// Video suppressed.
        /// </summary>
        DisableVideo
    }

    internal abstract class VideoEncoderImpl
    {
        public static VideoEncoderImpl Resolve(VideoEncoder encoder)
        {
            if (encoder == VideoEncoder.Copy) return Copy;
            if (encoder == VideoEncoder.DisableVideo) return DisableVideo;
            if (encoder == VideoEncoder.Libx264Fast) return Libx264Fast;
            if (encoder == VideoEncoder.Libx264Slow) return Libx264Slow;
            if (encoder == VideoEncoder.Libx264UltraFast) return Libx264UltraFast;

            throw new ArgumentException($"Unknown video encoder: '{encoder}'.");
        }

        public static VideoEncoderImpl Libx264Slow { get; } = new Libx264VideoEncoder("slow");
        public static VideoEncoderImpl Libx264Fast { get; } = new Libx264VideoEncoder("fast");
        public static VideoEncoderImpl Libx264UltraFast { get; } = new Libx264VideoEncoder("ultrafast");
        public static VideoEncoderImpl Copy { get; } = new CopyVideoEncoder();
        public static VideoEncoderImpl DisableVideo { get; } = new NoVideoEncoder();

        private VideoEncoderImpl()
        {
        }

        internal abstract string GetCliArgs(FFmpegClient ffmpeg);

        private sealed class Libx264VideoEncoder : VideoEncoderImpl
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

        sealed class CopyVideoEncoder : VideoEncoderImpl
        {
            internal override string GetCliArgs(FFmpegClient ffmpeg)
            {
                return "-c:v copy";
            }
        }

        sealed class NoVideoEncoder : VideoEncoderImpl
        {
            internal override string GetCliArgs(FFmpegClient ffmpeg)
            {
                return "-vn";
            }
        }
    }
}

#endif