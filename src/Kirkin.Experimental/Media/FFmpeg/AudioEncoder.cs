using System.Text;

namespace Kirkin.Media.FFmpeg
{
    public abstract class AudioEncoder
    {
        public static AudioEncoder AAC { get; } = new AacAudioEncoder();
        public static AudioEncoder Copy { get; } = new CopyAudioEncoder();

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
    }
}