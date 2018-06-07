using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Kirkin.Diagnostics;

namespace Kirkin.Media.FFmpeg
{
    /// <summary>
    /// Managed FFmpeg wrapper.
    /// </summary>
    public class FFmpegClient
    {
        /// <summary>
        /// ffmpeg.exe path specified when this instance was created.
        /// The default is null (use current directory/PATH).
        /// </summary>
        protected internal virtual string FFmpegPath { get; }

        /// <summary>
        /// Audio encoder. Use AAC for web video.
        /// </summary>
        public AudioEncoder AudioEncoder { get; set; }

        /// <summary>
        /// Target audio bitrate in Kbit/sec. Use 128 for 480p video. Use 192 for 720p, 1080p or above.
        /// </summary>
        public int AudioBitrate { get; set; }

        /// <summary>
        /// Number of audio channels. The default is zero (original).
        /// </summary>
        public int AudioChannels { get; set; }

        /// <summary>
        /// Video encoder. Use VideoEncoder.Libx264Fast/Libx264Slow for web video.
        /// </summary>
        public VideoEncoder VideoEncoder { get; set; }

        /// <summary>
        /// Target video bitrate in Kb/sec. The default is 0 (auto).
        /// A good guide is as follows:
        /// * 1080p (VideoHeight = 1080): ~2000 kpbs
        /// * 720p (VideoHeight = 720): ~1500 kbps
        /// * 480p (VideoHeight = 480): ~1000 kbps
        /// </summary>
        public int VideoBitrate { get; set; }

        /// <summary>
        /// Effective target video bitrate.
        /// </summary>
        protected internal virtual int TargetVideoBitrate
        {
            get
            {
                if (VideoBitrate != 0) return VideoBitrate;
                
                if (VideoHeight.HasValue)
                {
                    if (VideoHeight.Value >= 1440) return 2500;
                    if (VideoHeight.Value >= 1080) return 2000;
                    if (VideoHeight.Value >= 720) return 1500;
                    if (VideoHeight.Value >= 480) return 1000;

                    return 500;
                }

                return 0;
            }
        }

        /// <summary>
        /// Video width. The default is null (scale, preserve aspect ratio if VideoHeight is greater than zero).
        /// </summary>
        public int? VideoWidth { get; set; }

        /// <summary>
        /// Video height (usually 480, 720, 1080). The default is null (scale, preserve aspect ratio if VideoWidth is greater than zero).
        /// </summary>
        public int? VideoHeight { get; set; }

        /// <summary>
        /// Creates a new ffmpeg wrapper instance without specifying the exact ffmpeg.exe path.
        /// </summary>
        public FFmpegClient()
        {
        }

        /// <summary>
        /// Creates a new ffmpeg wrapper instance with the given ffmpeg.exe path.
        /// </summary>
        private FFmpegClient(string ffmpegPath)
        {
            FFmpegPath = ffmpegPath;
        }

        /// <summary>
        /// Converts the file at the given path using ffmpeg.exe.
        /// </summary>
        /// <param name="inputFilePath">Path to the input file.</param>
        /// <param name="outputFilePath">Path to the output file.</param>
        public void ConvertFile(string inputFilePath, string outputFilePath)
        {
            ProcessStartInfo info = new ProcessStartInfo(FFmpegPath ?? "ffmpeg", GetFFmpegArgString(inputFilePath, outputFilePath)) {
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            using (Process process = ChildProcess.Start(info, out bool associated))
            {
                process.EnableRaisingEvents = true;

                bool nonEmptyOutputSeen = false;

                process.OutputDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(e.Data) || nonEmptyOutputSeen)
                    {
                        nonEmptyOutputSeen = true;

                        Console.WriteLine(e.Data);
                    }
                };

                List<string> errors = new List<string>();

                process.ErrorDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        errors.Add(e.Data);
                        Console.WriteLine(e.Data);
                    }
                };

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    string errorText = (errors.Count > 1)
                        ? Environment.NewLine + string.Join(Environment.NewLine, errors)
                        : errors.FirstOrDefault();

                    throw new FFmpegException($"FFMpeg exited with code {process.ExitCode}. {errorText?.TrimStart(' ', ':')}", process.ExitCode);
                }
            }
        }

        private string GetFFmpegArgString(string inputFilePath, string outputFilePath)
        {
            return string.Join(" ", GetFFmpegArgs(inputFilePath, outputFilePath));
        }

        protected virtual IEnumerable<string> GetFFmpegArgs(string inputFilePath, string outputFilePath)
        {
            yield return $@"-i ""{inputFilePath}"""; // Input.

            foreach (string arg in GetFFmpegVideoArgs()) {
                yield return arg;
            }

            foreach (string arg in GetFFmpegAudioArgs()) {
                yield return arg;
            }

            yield return "-y"; // Overwrite files without prompting.
            yield return "-v warning"; // Output verbosity level.
            yield return $@"""{outputFilePath}"""; // Output.
        }

        protected virtual IEnumerable<string> GetFFmpegVideoArgs()
        {
            if (VideoEncoder != VideoEncoder.Auto) {
                yield return VideoEncoderImpl.Resolve(VideoEncoder).GetCliArgs(this);
            }

            int bitrate = TargetVideoBitrate;

            if (bitrate != 0)
            {
                yield return $"-b:v {bitrate}k";
                yield return $"-maxrate {bitrate}k";
                yield return $"-bufsize {bitrate * 2}k";
            }

            if (VideoWidth.HasValue || VideoHeight.HasValue) {
                // -2 means "auto, preserve aspect ratio.
                yield return $"-vf scale={VideoWidth ?? -2}:{VideoHeight ?? -2}";
            }
        }

        protected virtual IEnumerable<string> GetFFmpegAudioArgs()
        {
            if (AudioEncoder != AudioEncoder.Auto) {
                yield return AudioEncoderImpl.Resolve(AudioEncoder).GetCliArgs(this);
            }

            if (AudioBitrate != 0) {
                yield return "-b:a " + AudioBitrate + "k";
            }

            if (AudioChannels != 0) {
                yield return "-ac " + AudioChannels;
            }
        }

        public override string ToString()
        {
            return (FFmpegPath ?? "ffmpeg") + " " + GetFFmpegArgString("<input>", "<output>");
        }
    }
}