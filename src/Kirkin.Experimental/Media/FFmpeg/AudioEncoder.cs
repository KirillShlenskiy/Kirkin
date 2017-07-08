using System.Text;

namespace Kirkin.Media.FFmpeg
{
    /// <summary>
    /// ffmpeg audio encoder options.
    /// </summary>
    public abstract class AudioEncoder
    {
        /// <summary>
        /// AAC audio encoder.
        /// </summary>
        public static AudioEncoder AAC { get; } = new AacAudioEncoder();

        /// <summary>
        /// Passthrough audio encoder (direct stream copy).
        /// </summary>
        public static AudioEncoder Copy { get; } = new CopyAudioEncoder();

        /// <summary>
        /// Audio suppressed.
        /// </summary>
        public static AudioEncoder NoAudio { get; } = new NoAudioEncoder();

        private AudioEncoder()
        {
        }

        internal abstract string GetCliArgs(FFmpegClient ffmpeg);

        private sealed class AacAudioEncoder : AudioEncoder
        {
            internal override string GetCliArgs(FFmpegClient ffmpeg)
            {
                StringBuilder args = new StringBuilder("-c:a aac");

                if (ffmpeg.AudioBitrate != 0)
                    args.Append(" -b:a " + ffmpeg.AudioBitrate + "k");

                return args.ToString();
            }
        }

        sealed class CopyAudioEncoder : AudioEncoder
        {
            internal override string GetCliArgs(FFmpegClient ffmpeg)
            {
                return "-c:a copy";
            }
        }

        sealed class NoAudioEncoder : AudioEncoder
        {
            internal override string GetCliArgs(FFmpegClient ffmpeg)
            {
                return "-an";
            }
        }
    }
}