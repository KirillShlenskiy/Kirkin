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
        /// LAME MP3 audio encoder.
        /// </summary>
        public static AudioEncoder LibMp3Lame { get; } = new LibMp3LameAudioEncoder();

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
                return "-c:a aac";
            }
        }

        sealed class LibMp3LameAudioEncoder : AudioEncoder
        {
            internal override string GetCliArgs(FFmpegClient ffmpeg)
            {
                return "-c:a libmp3lame";
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