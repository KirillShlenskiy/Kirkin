using System;

namespace Kirkin.Media.FFmpeg
{
    /// <summary>
    /// ffmpeg audio encoder options.
    /// </summary>
    public enum AudioEncoder
    {
        /// <summary>
        /// Auto detection (based on output file extension).
        /// </summary>
        Auto,

        /// <summary>
        /// AAC audio encoder.
        /// </summary>
        AAC,

        /// <summary>
        /// LAME MP3 audio encoder.
        /// </summary>
        LibMp3Lame,

        /// <summary>
        /// Passthrough audio encoder (direct stream copy).
        /// </summary>
        Copy,

        /// <summary>
        /// Audio suppressed.
        /// </summary>
        DisableAudio
    }

    internal abstract class AudioEncoderImpl
    {
        public static AudioEncoderImpl Resolve(AudioEncoder encoder)
        {
            if (encoder == AudioEncoder.AAC) return AAC;
            if (encoder == AudioEncoder.Copy) return Copy;
            if (encoder == AudioEncoder.DisableAudio) return DisableAudio;
            if (encoder == AudioEncoder.LibMp3Lame) return LibMp3Lame;

            throw new ArgumentException($"Unknown audio encoder: '{encoder}'.");
        }

        public static AudioEncoderImpl AAC { get; } = new AacAudioEncoder();
        public static AudioEncoderImpl LibMp3Lame { get; } = new LibMp3LameAudioEncoder();
        public static AudioEncoderImpl Copy { get; } = new CopyAudioEncoder();
        public static AudioEncoderImpl DisableAudio { get; } = new NoAudioEncoder();

        private AudioEncoderImpl()
        {
        }

        internal abstract string GetCliArgs(FFmpegClient ffmpeg);

        private sealed class AacAudioEncoder : AudioEncoderImpl
        {
            internal override string GetCliArgs(FFmpegClient ffmpeg)
            {
                return "-c:a aac";
            }
        }

        sealed class LibMp3LameAudioEncoder : AudioEncoderImpl
        {
            internal override string GetCliArgs(FFmpegClient ffmpeg)
            {
                return "-c:a libmp3lame";
            }
        }

        sealed class CopyAudioEncoder : AudioEncoderImpl
        {
            internal override string GetCliArgs(FFmpegClient ffmpeg)
            {
                return "-c:a copy";
            }
        }

        sealed class NoAudioEncoder : AudioEncoderImpl
        {
            internal override string GetCliArgs(FFmpegClient ffmpeg)
            {
                return "-an";
            }
        }
    }
}